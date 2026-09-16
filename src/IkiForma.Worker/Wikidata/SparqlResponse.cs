namespace IkiForma.Worker.Wikidata;

public sealed class SparqlResponse
{
    public SparqlResults Results { get; set; } = new();
}

public sealed class SparqlResults
{
    public List<Dictionary<string, SparqlValue>> Bindings { get; set; } = [];
}

public sealed class SparqlValue
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}
