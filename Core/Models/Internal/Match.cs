namespace OMA.Core.Models.Internal;

public enum Team
{
    None,
    Blue,
    Red
}

public class Match
{
    public long LobbyId { get; set; }
    public string Name { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int BestOf { get; set; } = 0;
    public int WarmupCount { get; set; } = 0;
    public int RedWins { get; set; } = 0;
    public int BlueWins { get; set; } = 0;
    public Team WinningTeam { get; set; } = Team.None;
    public List<User> Users { get; set; } = [];
    public List<Map> CompletedMaps { get; set; } = [];
    public List<Map> ExtraMaps { get; set; } = [];
    public List<Map> WarmupMaps { get; set; } = [];
    public List<Map> AbandonedMaps { get; set; } = [];
}
