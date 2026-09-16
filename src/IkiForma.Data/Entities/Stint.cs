namespace IkiForma.Data.Entities;

/// <summary>Bir oyuncunun bir takımda oynadığı dönem. "İki takımda da oynamış oyuncu" sorgusunun temel tablosu.</summary>
public class Stint
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public DateOnly? StartDate { get; set; }
    public DatePrecision StartDatePrecision { get; set; } = DatePrecision.Unknown;

    /// <summary>Null = hâlâ devam ediyor ya da bitiş bilinmiyor.</summary>
    public DateOnly? EndDate { get; set; }
    public DatePrecision EndDatePrecision { get; set; } = DatePrecision.Unknown;

    public StintType StintType { get; set; } = StintType.Unknown;

    public int? Appearances { get; set; }
    public int? Goals { get; set; }

    /// <summary>Wikidata statement ID (ör. "Q615$3A2C...") — Worker'ın idempotent upsert'i için.</summary>
    public required string SourceStatementId { get; set; }
}
