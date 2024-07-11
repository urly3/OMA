namespace OMA.Models.Internal;

class Map
{
    public long BeatmapId { get; set; }
    public long BeatmapSetId { get; set; }
    public string CoverUrl { get; set; } = "oma_init";
    public string Mapper { get; set; } = "oma_init";
    public string Artist { get; set; } = "oma_init";
    public string Title { get; set; } = "oma_init";
    public float StarRating { get; set; } = 0.0f;
    public float AverageScore { get; set; } = 0.0f;
    public float BlueAverageScore { get; set; } = 0.0f;
    public float RedAverageScore { get; set; } = 0.0f;
    public List<Score> Scores { get; set; } = new();
}
