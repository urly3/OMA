namespace OMA.Core.Models.Imported;

public record Statistics
{
    public int count_100 { get; set; }
    public int count_300 { get; set; }
    public int count_50 { get; set; }
    public int count_geki { get; set; }
    public int count_katu { get; set; }
    public int count_miss { get; set; }
}
