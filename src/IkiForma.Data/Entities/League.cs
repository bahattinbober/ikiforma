namespace IkiForma.Data.Entities;

public class League
{
    public int Id { get; set; }

    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    public required string Name { get; set; }
    public string? Country { get; set; }

    public required string WikidataId { get; set; }

    public List<Team> Teams { get; set; } = [];
}
