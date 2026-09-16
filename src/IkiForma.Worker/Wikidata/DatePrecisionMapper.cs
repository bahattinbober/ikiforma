using IkiForma.Data.Entities;

namespace IkiForma.Worker.Wikidata;

internal static class DatePrecisionMapper
{
    /// <summary>Wikidata'nın wikibase:timePrecision kodu: 9=yıl, 10=ay, 11=gün.</summary>
    public static DatePrecision FromWikidataPrecision(int? precision) => precision switch
    {
        11 => DatePrecision.Day,
        10 => DatePrecision.Month,
        9 => DatePrecision.Year,
        _ => DatePrecision.Unknown
    };
}
