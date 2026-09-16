namespace IkiForma.Data.Entities;

public class Team
{
    public int Id { get; set; }

    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    /// <summary>Takımın güncel/birincil ligi. Basitleştirme: sezon bazlı geçmiş ileride ayrı bir ilişki tablosuna taşınabilir.</summary>
    public int? LeagueId { get; set; }
    public League? League { get; set; }

    public required string Name { get; set; }
    public string? Country { get; set; }

    public required string WikidataId { get; set; }

    public List<Stint> Stints { get; set; } = [];
}
