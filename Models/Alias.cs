
namespace OMA.Models;

public record Alias
{
    public int Id { get; set; }
    public List<Lobby> LobbyIds { get; set; } = [];
    public string Hash { get; set; } = default!;
    public string Password { get; set; } = default!;
    // public string Salt { get; set; } // if i feel like it?
    public bool Locked { get; set; } = false;
}
