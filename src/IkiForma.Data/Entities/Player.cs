namespace IkiForma.Data.Entities;

public class Player
{
    public int Id { get; set; }

    public required string FullName { get; set; }
    public DateOnly? BirthDate { get; set; }

    public required string WikidataId { get; set; }

    public List<Stint> Stints { get; set; } = [];
}
