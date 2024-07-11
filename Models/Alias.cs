namespace OMA.Models;

public record Alias
{
    public int Id { get; set; }
    public List<Lobby> LobbyIds { get; set; } = new();
    public bool Locked { get; set; }
    public string? Hash { get; set; }
    // public string Salt { get; set; } // if i feel like it?
}
