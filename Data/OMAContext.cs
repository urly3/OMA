using Microsoft.EntityFrameworkCore;
using OMA.Models;

namespace OMA.Data;

public class OMAContext : DbContext
{
    private string _connectionString = "Data Source=OMADB.db";
    public DbSet<Lobby> Lobbies { get; set; } = default!;
    public DbSet<Alias> Aliases { get; set; } = default!;

    public OMAContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }
}
