using System.Globalization;

namespace IkiForma.Worker.Wikidata;

public static class SparqlRowExtensions
{
    public static string? GetString(this Dictionary<string, SparqlValue> row, string key) =>
        row.TryGetValue(key, out var v) ? v.Value : null;

    /// <summary>Bir URI değerinin son path parçasını döner (ör. QID veya statement ID).</summary>
    public static string? GetId(this Dictionary<string, SparqlValue> row, string key)
    {
        var uri = row.GetString(key);
        if (uri is null) return null;
        var idx = uri.LastIndexOf('/');
        return idx >= 0 ? uri[(idx + 1)..] : uri;
    }

    public static int? GetInt(this Dictionary<string, SparqlValue> row, string key)
    {
        var s = row.GetString(key);
        if (s is null) return null;
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return n;
        // appearances/goals xsd:decimal olarak gelir; nadiren "33.0" gibi ondalıklı yazılabilir.
        return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? (int)d : null;
    }

    public static DateOnly? GetDate(this Dictionary<string, SparqlValue> row, string key)
    {
        var s = row.GetString(key);
        if (s is null) return null;
        return DateOnly.FromDateTime(DateTimeOffset.Parse(s, CultureInfo.InvariantCulture).UtcDateTime);
    }
}
