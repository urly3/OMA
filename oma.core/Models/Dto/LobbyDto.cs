namespace OMA.Models.Dto;

public class LobbyDTO
{
    public int Id { get; set; } = -1;
    public long LobbyId { get; set; }
    public int BestOf { get; set; }
    public int Warmups { get; set; }

    internal LobbyDTO(Lobby? lobby)
    {
        if (lobby == null)
        {
            return;
        }

        Id = lobby.Id;
        LobbyId = lobby.LobbyId;
        BestOf = lobby.BestOf;
        Warmups = lobby.Warmups;
    }
}

