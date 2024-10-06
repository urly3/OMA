namespace OMA.Models.Dto;

public class AliasDto
{
    public int Id { get; set; } = -1;
    public List<LobbyDto> Lobbies { get; set; } = [];
    public bool Locked { get; set; } = false;

    internal AliasDto(Alias? alias)
    {
        if (alias == null)
        {
            return;
        }

        Id = alias.Id;
        Locked = alias.Password != null;

        foreach (var lobby in alias.Lobbies)
        {
            Lobbies.Add(new(lobby));
        }
    }
}
