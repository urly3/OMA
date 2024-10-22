namespace OMA.Core.Models;

public record Alias
{
    public int Id { get; set; }
    public List<Lobby> Lobbies { get; set; } = [];
    public string Hash { get; set; } = default!;
    public string? Password { get; set; } = null;
    // no need for locked bool. if it has a password, it's locked.
    // prevent unnecessary data store and race potential.
}
