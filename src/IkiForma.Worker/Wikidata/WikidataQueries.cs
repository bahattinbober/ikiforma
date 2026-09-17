namespace IkiForma.Worker.Wikidata;

internal static class WikidataQueries
{
    public const string SuperLigQid = "Q485568";

    /// <summary>"association football player" — P54 kayıtlarını futbolcu olmayan üyelerden (çok branşlı kulüplerde yönetici/başka spor sporcusu) ayıklamak için.</summary>
    private const string FootballerQid = "Q937857";

    /// <summary>
    /// Verilen takım QID'lerini (bkz. src/IkiForma.Worker/Data/superlig-teams.json) Wikidata'dan
    /// güncel etiket ve ülke bilgisiyle çeker.
    ///
    /// Dinamik keşif (wdt:P118 güncel lig + wdt:P3450/P1923 sezon union'ı) terk edildi: Wikidata'da
    /// bazı tarihi Süper Lig kulüplerinin sezon verisi eksik/tutarsız olduğu için dinamik sorgu
    /// tr.wikipedia'nın "tüm zamanlar" listesindeki 79 kulübün önemli bir kısmını kaçırıyordu.
    /// Statik liste elle doğrulanıp (P54 oyuncu sayısı ile) her QID'nin gerçekten o kulübe ait
    /// olduğu teyit edildikten sonra oluşturuldu. "Futbol kulübü" tip filtresi kasıtlı olarak
    /// kaldırıldı: Karşıyaka gibi çok branşlı kulüpler Wikidata'da "sports club" olarak
    /// modellenebiliyor ve bu filtre onları düşürüyordu — liste zaten elle doğrulandığı için
    /// filtreye gerek yok (bkz. WikidataSyncService'teki eksik-QID uyarısı).
    /// </summary>
    public static string TeamsByQids(IEnumerable<string> qids) => $$"""
        SELECT DISTINCT ?team ?teamLabel ?countryLabel WHERE {
          VALUES ?team { {{string.Join(" ", qids.Select(q => $"wd:{q}"))}} }
          OPTIONAL { ?team wdt:P17 ?country. }
          SERVICE wikibase:label { bd:serviceParam wikibase:language "tr,en". }
        }
        """;

    /// <summary>Verilen takımda P54 (member of sports team) ile geçen futbolcular ve stint nitelikleri.</summary>
    public static string Stints(string teamQid) => $$"""
        SELECT ?player ?playerLabel ?membership
               ?start ?startPrecision ?end ?endPrecision
               ?transferType ?appearances ?goals
        WHERE {
          ?player wdt:P31 wd:Q5;
                  wdt:P106 wd:{{FootballerQid}};
                  p:P54 ?membership.
          ?membership ps:P54 wd:{{teamQid}}.

          OPTIONAL {
            ?membership pqv:P580 ?startNode.
            ?startNode wikibase:timeValue ?start; wikibase:timePrecision ?startPrecision.
          }
          OPTIONAL {
            ?membership pqv:P582 ?endNode.
            ?endNode wikibase:timeValue ?end; wikibase:timePrecision ?endPrecision.
          }
          OPTIONAL { ?membership pq:P1642 ?transferType. }
          OPTIONAL { ?membership pq:P1350 ?appearances. }
          OPTIONAL { ?membership pq:P1351 ?goals. }
          SERVICE wikibase:label { bd:serviceParam wikibase:language "tr,en". }
        }
        """;
}
