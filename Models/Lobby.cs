namespace OMA.Models;

public record Lobby
{
    public int Id { get; set; }
    public int LobbyId { get; set; }
    public int BestOf { get; set; } = 0;
    public int Warmups { get; set; } = 0;
    public List<Alias> Aliases { get; set; } = [];
}
