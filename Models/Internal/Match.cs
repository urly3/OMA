namespace OMA.Models.Internal;

enum Team
{
    None,
    Blue,
    Red,
}

class Match
{
    public int Id { get; set; }
    public long MultiId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int BestOf { get; set; } = 0;
    public int WarmupCount { get; set; } = 0;
    public int RedWins { get; set; } = 0;
    public int BlueWins { get; set; } = 0;
    public Team WinningTeam { get; set; } = Team.None;
    public List<User> Users { get; set; } = new();
    public List<Map> CompletedMaps { get; set; } = new();
    public List<Map> ExtraMaps { get; set; } = new();
    public List<Map> WarmupMaps { get; set; } = new();
    public List<Map> AbandonedMaps { get; set; } = new();
}
