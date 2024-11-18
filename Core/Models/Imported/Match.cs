namespace OMA.Core.Models.Imported;

public record Match
{
    public long id { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
    public string? name { get; set; }
    public int slot { get; set; }
    public string? team { get; set; }
    public bool pass { get; set; }
}
