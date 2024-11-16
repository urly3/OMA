using System.ComponentModel.DataAnnotations;

namespace OMA.Core.Models;

public class Lobby
{
    public int Id { get; set; }
    public long LobbyId { get; set; }
    [MaxLength(100)] public string LobbyName { get; set; } = default!;
    public int BestOf { get; set; } = 0;
    public int Warmups { get; set; } = 0;
    public List<Alias> Aliases { get; set; } = [];
}