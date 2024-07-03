namespace OMA.Models.Internal;

class Map
{
    public int Id { get; set; }
    public long BeatmapId { get; set; }
    public long  BeatmapSetId { get; set; }
    public string CoverUrl { get; set; } = "oma_init";
    public string Mapper { get; set; } =  "oma_init";
    public string Artist { get; set; } = "oma_init";
    public string Title { get; set; } = "oma_init";
    public float StarRating { get; set; }
    public List<Score> Scores { get; set; } = new();
}
