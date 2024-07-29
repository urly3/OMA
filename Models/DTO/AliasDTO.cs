using OMA.Models;

namespace OMA.Models.Dto;

public class AliasDto
{
    public int Id { get; set; } = -1;
    public List<Lobby> Lobbies { get; set; } = [];
    public bool Locked { get; set; } = false;

    internal AliasDto(Alias? alias)
    {
        if (alias == null)
        {
            return;
        }

        Id = alias.Id;
        Lobbies = alias.Lobbies;
        Locked = alias.Password != null;
    }
}