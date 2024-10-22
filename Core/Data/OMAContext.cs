using Microsoft.EntityFrameworkCore;
using OMA.Core.Models;

namespace OMA.Core.Data;

public class OMAContext : DbContext
{
    private string _connectionString = "Data Source=oma.db";
    public DbSet<Lobby> Lobbies { get; set; } = default!;
    public DbSet<Alias> Aliases { get; set; } = default!;

    public OMAContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }

    internal Alias? GetAlias(string aliasHash)
    {
        var alias = Aliases.Include(a => a.Lobbies).FirstOrDefault(a => a.Hash == aliasHash);
        if (alias == null)
        {
            return null;
        }

        return alias;
    }

    internal Alias? GetAliasById(int id)
    {
        return Aliases.Include(a => a.Lobbies).FirstOrDefault(a => a.Id == id);
    }

    internal Alias? CreateAlias(string aliasHash)
    {
        var alias = Aliases.Add(new()
        {
            Hash = aliasHash,
        });

        try
        {
            SaveChanges();
        }
        catch
        {
            return null;
        }

        alias.Reload();
        return alias.Entity;
    }

    internal bool AddLobbyToAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Add(lobby);
        Aliases.Update(alias);

        try
        {
            SaveChanges();
        }
        catch
        {
            return false;
        }

        return true;
    }

    internal bool RemoveLobbyFromAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Remove(lobby);
        Aliases.Update(alias);

        try
        {
            SaveChanges();
        }
        catch
        {
            return false;
        }

        return true;
    }

    internal bool SetAliasPassword(Alias alias, string passwordHash)
    {
        alias.Password = passwordHash;
        Aliases.Update(alias);

        try
        {
            SaveChanges();
        }
        catch
        {
            return false;
        }

        return true;
    }

    internal bool RemoveAliasPassword(Alias alias)
    {
        alias.Password = null;
        Aliases.Update(alias);

        try
        {
            SaveChanges();
        }
        catch
        {
            return false;
        }

        return true;
    }

    internal Lobby? GetLobbyEqual(string lobbyName, long lobbyId, int bestOf, int warmups)
    {
        return Lobbies.FirstOrDefault(l => l.LobbyName == lobbyName && l.LobbyId == lobbyId && l.BestOf == bestOf && l.Warmups == warmups);
    }
}
