namespace OMA.Core.Models.Internal;

public class User
{
    public long UserId { get; set; }
    public string Username { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;
    public string CountryName { get; set; } = default!;
    public string CountryCode { get; set; } = default!;

    public Team Team { get; set; } = Team.None;
    public int MapsPlayed { get; set; } = 0;

    public float MatchCostTeam { get; set; }
    public float MatchCostTotal { get; set; }

    public long AverageScore { get; set; }
    public long HighestScore { get; set; }
    public long LowestScore { get; set; }

    public float AverageAccuracy { get; set; }
    public float HighestAccuracy { get; set; }
    public float LowestAccuracy { get; set; }
}
