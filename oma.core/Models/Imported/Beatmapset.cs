namespace OMA.Models.Imported;

public record Beatmapset {
    public string? artist { get; set; }
    public string? artist_unicode { get; set; }
    public Covers? covers { get; set; }
    public string? creator { get; set; }
    public int favourite_count { get; set; }
    public object? hype { get; set; }
    public long id { get; set; }
    public bool nsfw { get; set; }
    public long offset { get; set; }
    public long play_count { get; set; }
    public string? preview_url { get; set; }
    public string? source { get; set; }
    public bool spotlight { get; set; }
    public string? status { get; set; }
    public string? title { get; set; }
    public string? title_unicode { get; set; }
    public long? track_id { get; set; }
    public long user_id { get; set; }
    public bool video { get; set; }
}
