using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IkiForma.Worker.Wikidata;

/// <summary>
/// query.wikidata.org SPARQL endpoint'ine ince bir sarmalayıcı. User-Agent ve Accept
/// header'ları DI kaydında (Program.cs) sabitlenir, burada tekrar edilmez.
/// </summary>
public sealed class WikidataClient(HttpClient httpClient, ILogger<WikidataClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int MaxAttempts = 4;

    public async Task<List<Dictionary<string, SparqlValue>>> QueryAsync(string sparql, CancellationToken ct)
    {
        var url = $"sparql?query={Uri.EscapeDataString(sparql)}&format=json";

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<SparqlResponse>(url, JsonOptions, ct);
                return response?.Results.Bindings ?? [];
            }
            catch (Exception ex) when (attempt < MaxAttempts && !ct.IsCancellationRequested && IsTransient(ex))
            {
                // WDQS yoğunluk altında ara sıra 502/503/429 döner ya da HttpClient.Timeout dolar
                // (bu TaskCanceledException fırlatır, kendi ct'imizin iptaliyle karıştırılmamalı).
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning(
                    "Wikidata sorgusu başarısız oldu (deneme {Attempt}/{Max}), {Delay}s sonra tekrar denenecek: {Message}",
                    attempt, MaxAttempts, delay.TotalSeconds, ex.Message);
                await Task.Delay(delay, ct);
            }
        }
    }

    private static bool IsTransient(Exception ex) => ex switch
    {
        HttpRequestException { StatusCode: HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout
            or HttpStatusCode.TooManyRequests
            or null } => true,  // null = yanıt hiç alınamadı (bağlantı/DNS seviyesinde hata)
        TaskCanceledException => true,  // HttpClient.Timeout doldu (ct iptali yukarıda zaten elendi)
        _ => false
    };
}
