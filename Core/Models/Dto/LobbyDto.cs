namespace OMA.Core.Models.Dto;

public class LobbyDto
{
    public int Id { get; set; } = -1;
    public long LobbyId { get; set; }
    public string LobbyName { get; set; } = "";
    public int BestOf { get; set; }
    public int Warmups { get; set; }

    internal LobbyDto(Lobby? lobby)
    {
        if (lobby == null)
        {
            return;
        }

        Id = lobby.Id;
        LobbyId = lobby.LobbyId;
        LobbyName = lobby.LobbyName;
        BestOf = lobby.BestOf;
        Warmups = lobby.Warmups;
    }
}

