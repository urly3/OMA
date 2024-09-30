using Microsoft.EntityFrameworkCore;
using System.IO.Hashing;
using OMA.Models;

namespace OMA.Data;

public class OMAContext : DbContext {
    private string _connectionString = "Data Source=oma.db";
    public DbSet<Lobby> Lobbies { get; set; } = default!;
    public DbSet<Alias> Aliases { get; set; } = default!;

    public OMAContext() {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite(_connectionString);
    }

    internal Alias? GetAlias(string name) {
        var hash = HashString(name);

        var alias = Aliases.Include(a => a.Lobbies).FirstOrDefault(a => a.Hash == hash);
        if (alias == null) {
            return null;
        }

        return alias;
    }

    internal Alias? GetAliasFromHash(string hash) {
        var alias = Aliases.Include(a => a.Lobbies).FirstOrDefault(a => a.Hash == hash);
        if (alias == null) {
            return null;
        }

        return alias;
    }

    internal Alias? GetAliasById(int id) {
        return Aliases.Include(a => a.Lobbies).FirstOrDefault(a => a.Id == id);
    }

    internal bool CreateAlias(string name) {
        var hash = HashString(name);

        Aliases.Add(new() {
            Hash = hash,
        });

        return SaveChanges() != 0;
    }

    internal bool CreateAliasFromHash(string aliasHash) {
        Aliases.Add(new() {
            Hash = aliasHash,
        });

        return SaveChanges() != 0;
    }

    internal bool AddLobbyToAlias(Alias alias, Lobby lobby) {
        alias.Lobbies.Add(lobby);
        Aliases.Update(alias);

        return SaveChanges() != 0;
    }

    internal bool RemoveLobbyFromAlias(Alias alias, Lobby lobby) {
        alias.Lobbies.Remove(lobby);
        Aliases.Update(alias);

        return SaveChanges() != 0;
    }

    internal bool SetAliasPassword(Alias alias, string password) {
        var hash = HashString(password);

        alias.Password = hash;
        Aliases.Update(alias);

        return SaveChanges() != 0;
    }

    internal bool RemoveAliasPassword(Alias alias) {
        alias.Password = null;
        Aliases.Update(alias);

        return SaveChanges() != 0;
    }

    internal string HashString(string value) {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(value));
        return System.Text.Encoding.UTF8.GetString(hashBytes);
    }

    internal Lobby? GetLobbyEqual(long lobbyId, int bestOf, int warmups) {
        return Lobbies.FirstOrDefault(l => l.LobbyId == lobbyId && l.BestOf == bestOf && l.Warmups == warmups);
    }
}
