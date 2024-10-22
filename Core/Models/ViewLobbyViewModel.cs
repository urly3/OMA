using OMA.Core.Models.Dto;
using OMA.Core.Models.Internal;

namespace OMA.Core.Models;

public class ViewLobbyViewModel
{
    public AliasDto? AliasDto { get; set; }
    public Match? Match { get; set; }
    public User[] BlueTeam { get; set; }
    public User[] RedTeam { get; set; }

    public ViewLobbyViewModel(AliasDto? dto, Match? mtc)
    {
        AliasDto = dto;
        Match = mtc;
        if (Match == null)
        {
            BlueTeam = [];
            RedTeam = [];
        }
        else
        {
            foreach (var user in Match.Users)
            {
                user.AverageAccuracy = (float)Math.Round(user.AverageAccuracy, 2);
                user.HighestAccuracy = (float)Math.Round(user.HighestAccuracy, 2);
                user.LowestAccuracy = (float)Math.Round(user.LowestAccuracy, 2);

                user.MatchCostTotal = (float)Math.Round(user.MatchCostTotal, 2);
                user.MatchCostTeam = (float)Math.Round(user.MatchCostTeam, 2);
            }
            BlueTeam = Match.Users.Where(u => u.Team == Team.Blue).OrderByDescending(u => u.MatchCostTotal).ToArray();
            RedTeam = Match.Users.Where(u => u.Team == Team.Red).OrderByDescending(u => u.MatchCostTotal).ToArray();
        }
    }
}