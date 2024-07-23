namespace OMA.Models;

public record Lobby
{
    public int Id { get; set; }
    public int LobbyId { get; set; }
    public List<Alias> Aliases { get; set; } = [];
}
