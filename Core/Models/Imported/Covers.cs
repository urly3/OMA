using System.Text.Json.Serialization;

namespace OMA.Core.Models.Imported;

public record Covers
{
    public string? cover { get; set; }

    [JsonPropertyName("cover@2x")] public string? cover2x { get; set; }
    public string? card { get; set; }

    [JsonPropertyName("card@2x")] public string? card2x { get; set; }
    public string? list { get; set; }

    [JsonPropertyName("list@2x")] public string? list2x { get; set; }
    public string? slimcover { get; set; }

    [JsonPropertyName("slimcover@2x")] public string? slimcover2x { get; set; }
}
