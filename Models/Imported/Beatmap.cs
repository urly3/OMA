namespace OMA.Models.Imported;

public record Beatmap
{
    public long beatmapset_id { get; set; }
    public float difficulty_rating { get; set; }
    public long id { get; set; }
    public string? mode { get; set; }
    public string? status { get; set; }
    public long total_length { get; set; }
    public long user_id { get; set; }
    public string? version { get; set; }
    public Beatmapset? beatmapset { get; set; }
}
