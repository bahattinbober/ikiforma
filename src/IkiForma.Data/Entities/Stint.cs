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
    public DateOnly? EndDate { get; set; }
}
