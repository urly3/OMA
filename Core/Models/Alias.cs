using System.ComponentModel.DataAnnotations;

namespace OMA.Core.Models;

public class Alias
{
    public int Id { get; set; }
    public List<Lobby> Lobbies { get; set; } = [];
    [MaxLength(100)] public string Hash { get; set; } = default!;
    [MaxLength(100)] public string? Password { get; set; } = null;
}
