namespace IkiForma.Data.Entities;

public class Sport
{
    public int Id { get; set; }

    /// <summary>Sabit makine anahtarı, örn. "football". Frontend yerelleştirmesi bu anahtar üzerinden yapılır.</summary>
    public required string Code { get; set; }

    public required string Name { get; set; }

    public List<League> Leagues { get; set; } = [];
    public List<Team> Teams { get; set; } = [];
}
