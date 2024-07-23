using Microsoft.EntityFrameworkCore;
using OMA.Models;

namespace OMA.Data;

class OMAContext : DbContext
{
    public DbSet<Lobby> Lobbies { get; set; } = default!;
    public DbSet<Alias> Aliases { get; set; } = default!;
}
