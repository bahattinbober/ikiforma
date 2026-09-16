using IkiForma.Data.Entities;

namespace IkiForma.Worker.Wikidata;

/// <summary>
/// P1642 (transfer işlemi) değerini QID üzerinden eşler (etiket metniyle değil — etiket dile
/// göre değişir, QID değişmez). QID'ler Süper Lig verisinde çalıştırılan bir keşif sorgusuyla
/// doğrulandı: https://query.wikidata.org/ üzerinde COUNT ile gruplanarak tek tek kontrol edildi.
/// </summary>
internal static class StintTypeMapper
{
    private const string Loan = "Q2914547";           // kiralama
    private const string Transfer = "Q1811518";        // transfer
    private const string FreeTransfer = "Q3622633";    // serbest transfer
    private const string FreeAgent = "Q969772";        // serbest oyuncu

    public static StintType FromWikidataQid(string? qid) => qid switch
    {
        Loan => StintType.Loan,
        Transfer or FreeTransfer or FreeAgent => StintType.Permanent,
        _ => StintType.Unknown
    };
}
