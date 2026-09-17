using System.Text.Json;
using IkiForma.Data;
using IkiForma.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkiForma.Worker.Wikidata;

/// <summary>
/// Süper Lig takımlarını, oyuncularını ve stint'lerini Wikidata'dan çekip veritabanına
/// idempotent şekilde upsert eder. Takım QID'leri Data/superlig-teams.json'dan okunur
/// (bkz. WikidataQueries.TeamsByQids). Tek seferlik çalışıp host'u durdurur (zamanlama sonraki adım).
/// </summary>
public sealed class WikidataSyncService(
    IServiceScopeFactory scopeFactory,
    WikidataClient wikidataClient,
    IHostApplicationLifetime lifetime,
    ILogger<WikidataSyncService> logger) : BackgroundService
{
    private static readonly TimeSpan DelayBetweenTeams = TimeSpan.FromSeconds(1);

    /// <summary>Seed dosyası bozuk/boş okunursa (ör. deserialize hatası) tüm takımları silmeyi engelleyen alt sınır.</summary>
    private const int MinSeedTeamCount = 50;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record TeamSeed(string Qid, string Name);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Wikidata senkronizasyonu başarısız oldu.");
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IkiFormaDbContext>();

        var sport = await SeedSportAsync(db, ct);
        var league = await SeedLeagueAsync(db, sport, ct);
        logger.LogInformation("Sport/League hazır: {Sport} / {League}.", sport.Code, league.Name);

        var teamQids = await LoadSeedTeamQidsAsync(ct);
        var teamRows = await wikidataClient.QueryAsync(WikidataQueries.TeamsByQids(teamQids), ct);
        WarnIfTeamsMissing(teamQids, teamRows, logger);

        var (teams, newTeamCount) = await UpsertTeamsAsync(db, teamRows, sport, league, ct);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Takımlar işlendi: {Total} toplam, {New} yeni.", teams.Count, newTeamCount);

        var existingPlayers = await db.Players.ToDictionaryAsync(p => p.WikidataId, ct);
        var existingStints = await db.Stints.ToDictionaryAsync(s => s.SourceStatementId, ct);
        var newPlayerCount = 0;
        var newStintCount = 0;
        var totalStintRows = 0;

        foreach (var team in teams)
        {
            ct.ThrowIfCancellationRequested();

            var rows = await wikidataClient.QueryAsync(WikidataQueries.Stints(team.WikidataId), ct);

            foreach (var row in rows)
            {
                var playerQid = row.GetId("player");
                var playerName = row.GetString("playerLabel");
                var statementId = row.GetId("membership");
                if (playerQid is null || playerName is null || statementId is null) continue;

                if (!existingPlayers.TryGetValue(playerQid, out var player))
                {
                    player = new Player { FullName = playerName, WikidataId = playerQid };
                    db.Players.Add(player);
                    existingPlayers[playerQid] = player;
                    newPlayerCount++;
                }
                else if (player.FullName != playerName)
                {
                    player.FullName = playerName;
                }

                totalStintRows++;

                if (!existingStints.TryGetValue(statementId, out var stint))
                {
                    stint = new Stint { Player = player, Team = team, SourceStatementId = statementId };
                    db.Stints.Add(stint);
                    existingStints[statementId] = stint;
                    newStintCount++;
                }

                stint.StartDate = row.GetDate("start");
                stint.StartDatePrecision = DatePrecisionMapper.FromWikidataPrecision(row.GetInt("startPrecision"));
                stint.EndDate = row.GetDate("end");
                stint.EndDatePrecision = DatePrecisionMapper.FromWikidataPrecision(row.GetInt("endPrecision"));
                stint.StintType = StintTypeMapper.FromWikidataQid(row.GetId("transferType"));
                stint.Appearances = row.GetInt("appearances");
                stint.Goals = row.GetInt("goals");
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("{Team}: {Count} P54 kaydı işlendi.", team.Name, rows.Count);

            await Task.Delay(DelayBetweenTeams, ct);
        }

        logger.LogInformation(
            "Wikidata senkronizasyonu tamamlandı. Takım: {TeamTotal} ({TeamNew} yeni) · " +
            "Oyuncu: {PlayerTotal} ({PlayerNew} yeni) · Stint: {StintTotal} ({StintNew} yeni).",
            teams.Count, newTeamCount,
            existingPlayers.Count, newPlayerCount,
            totalStintRows, newStintCount);

        await RemoveStaleDataAsync(db, teamQids, logger, ct);
    }

    /// <summary>Seed dosyasından çıkarılmış takımları (ve kaskad ile stint'lerini) siler, ardından hiç stint'i kalmayan oyuncuları temizler.</summary>
    private static async Task RemoveStaleDataAsync(
        IkiFormaDbContext db, List<string> seedQids, ILogger logger, CancellationToken ct)
    {
        if (seedQids.Count < MinSeedTeamCount)
        {
            logger.LogWarning(
                "Seed listesi sadece {Count} QID içeriyor (< {Min}), temizlik adımı güvenlik için atlandı.",
                seedQids.Count, MinSeedTeamCount);
            return;
        }

        var seedQidSet = seedQids.ToHashSet();
        var removedTeams = await db.Teams
            .Where(t => !seedQidSet.Contains(t.WikidataId))
            .ExecuteDeleteAsync(ct);
        if (removedTeams > 0)
            logger.LogInformation("Seed dışı {Count} takım (ve kaskad ile stint'leri) silindi.", removedTeams);

        var removedPlayers = await db.Players
            .Where(p => !db.Stints.Any(s => s.PlayerId == p.Id))
            .ExecuteDeleteAsync(ct);
        if (removedPlayers > 0)
            logger.LogInformation("Hiç stint'i kalmayan {Count} oyuncu silindi.", removedPlayers);
    }

    private static void WarnIfTeamsMissing(
        List<string> seedQids, List<Dictionary<string, SparqlValue>> teamRows, ILogger logger)
    {
        var returnedQids = teamRows.Select(r => r.GetId("team")).Where(q => q is not null).ToHashSet();
        var missingQids = seedQids.Where(q => !returnedQids.Contains(q)).ToList();
        if (missingQids.Count > 0)
        {
            logger.LogWarning(
                "Seed dosyasında olup Wikidata'dan dönmeyen {Count} QID (silinmiş/birleştirilmiş olabilir): {Qids}",
                missingQids.Count, string.Join(", ", missingQids));
        }
    }

    private static async Task<List<string>> LoadSeedTeamQidsAsync(CancellationToken ct)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "superlig-teams.json");
        var json = await File.ReadAllTextAsync(path, ct);
        var seeds = JsonSerializer.Deserialize<List<TeamSeed>>(json, JsonOptions) ?? [];
        return seeds.Select(s => s.Qid).ToList();
    }

    private static async Task<Sport> SeedSportAsync(IkiFormaDbContext db, CancellationToken ct)
    {
        var sport = await db.Sports.FirstOrDefaultAsync(s => s.Code == "football", ct);
        if (sport is not null) return sport;

        sport = new Sport { Code = "football", Name = "Football" };
        db.Sports.Add(sport);
        await db.SaveChangesAsync(ct);
        return sport;
    }

    private static async Task<League> SeedLeagueAsync(IkiFormaDbContext db, Sport sport, CancellationToken ct)
    {
        var league = await db.Leagues.FirstOrDefaultAsync(l => l.WikidataId == WikidataQueries.SuperLigQid, ct);
        if (league is not null) return league;

        league = new League
        {
            WikidataId = WikidataQueries.SuperLigQid,
            Name = "Süper Lig",
            Country = "Türkiye",
            Sport = sport
        };
        db.Leagues.Add(league);
        await db.SaveChangesAsync(ct);
        return league;
    }

    private static async Task<(List<Team> Teams, int NewCount)> UpsertTeamsAsync(
        IkiFormaDbContext db,
        List<Dictionary<string, SparqlValue>> rows,
        Sport sport,
        League league,
        CancellationToken ct)
    {
        var existing = await db.Teams.ToDictionaryAsync(t => t.WikidataId, ct);
        var teams = new List<Team>();
        var newCount = 0;

        foreach (var row in rows)
        {
            var qid = row.GetId("team");
            var name = row.GetString("teamLabel");
            var country = row.GetString("countryLabel");
            if (qid is null || name is null) continue;

            if (!existing.TryGetValue(qid, out var team))
            {
                team = new Team { WikidataId = qid, Name = name, Country = country, Sport = sport, League = league };
                db.Teams.Add(team);
                existing[qid] = team;
                newCount++;
            }
            else
            {
                team.Name = name;
                team.Country = country;
            }

            teams.Add(team);
        }

        return (teams, newCount);
    }
}
