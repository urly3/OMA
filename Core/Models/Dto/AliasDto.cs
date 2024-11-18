namespace OMA.Core.Models.Dto;

public class AliasDto
{
    internal AliasDto(Alias? alias)
    {
        if (alias == null) return;

        Id = alias.Id;
        Locked = alias.Password != null;

        foreach (var lobby in alias.Lobbies) Lobbies.Add(new LobbyDto(lobby));
    }

    public int Id { get; set; } = -1;
    public List<LobbyDto> Lobbies { get; set; } = [];
    public bool Locked { get; set; }
}
