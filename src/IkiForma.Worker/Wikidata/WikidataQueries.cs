namespace IkiForma.Worker.Wikidata;

internal static class WikidataQueries
{
    public const string SuperLigQid = "Q485568";

    /// <summary>
    /// "association football club" — P118/P1923'ün takım DIŞINDA (ör. oyuncu) sonuç
    /// döndürmesini engellemek için zorunlu.
    /// </summary>
    private const string FootballClubQid = "Q476028";

    /// <summary>
    /// Süper Lig'deki takımlar. wdt:P118 sadece takımın GÜNCEL ligini verir; sezon bazlı
    /// P3450+P1923 yaklaşımı geçmiş sezonları da kapsar. İkisinin UNION'ı en eksiksiz
    /// tarihsel listeyi verir.
    ///
    /// ÖNEMLİ: P118 Wikidata'da "league in which TEAM OR PLAYER plays" olarak tanımlı —
    /// yani doğrudan futbolcu item'larına da uygulanır. wdt:P31/wdt:P279* ile "futbol
    /// kulübü" (Q476028) filtresi olmadan bu sorgu takım yerine oyuncu/maç item'ları da
    /// döndürür (ilk çalıştırmada fark edildi: 103 "takım"dan sadece ~31'i gerçekti).
    /// </summary>
    public static string Teams => $$"""
        SELECT DISTINCT ?team ?teamLabel ?countryLabel WHERE {
          {
            ?team wdt:P118 wd:{{SuperLigQid}};
                  wdt:P31/wdt:P279* wd:{{FootballClubQid}}.
          }
          UNION
          {
            ?season wdt:P3450 wd:{{SuperLigQid}};
                    wdt:P1923 ?team.
            ?team wdt:P31/wdt:P279* wd:{{FootballClubQid}}.
          }
          OPTIONAL { ?team wdt:P17 ?country. }
          SERVICE wikibase:label { bd:serviceParam wikibase:language "tr,en". }
        }
        LIMIT 1000
        """;

    /// <summary>Verilen takımda P54 (member of sports team) ile geçen oyuncular ve stint nitelikleri.</summary>
    public static string Stints(string teamQid) => $$"""
        SELECT ?player ?playerLabel ?membership
               ?start ?startPrecision ?end ?endPrecision
               ?transferType ?appearances ?goals
        WHERE {
          ?player wdt:P31 wd:Q5;
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
