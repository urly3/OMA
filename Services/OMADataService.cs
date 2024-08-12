using OMA.Models;
using System.IO.Hashing;
using Microsoft.EntityFrameworkCore;

namespace OMA.Data;

public class OMADataService
{
    private OMAContext _context;

    public OMADataService()
    {
        _context = new OMAContext();
    }

    internal Alias? GetAlias(string name)
    {
        var hash = HashString(name);

        var alias = _context.Aliases.Include("Lobbies").FirstOrDefault(a => a.Hash == hash);
        if (alias == null)
        {
            return null;
        }

        return alias;
    }

    internal Alias? GetAliasById(int id)
    {
        return _context.Aliases.Include("Lobbies").FirstOrDefault(a => a.Id == id);
    }

    internal bool CreateAlias(string name)
    {
        var hash = HashString(name);

        _context.Aliases.Add(new()
        {
            Hash = hash,
        });

        return _context.SaveChanges() != 0;
    }

    internal bool AddLobbyToAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Add(lobby);
        _context.Aliases.Update(alias);

        return _context.SaveChanges() != 0;
    }

    internal bool RemoveLobbyFromAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Remove(lobby);
        _context.Aliases.Update(alias);

        return _context.SaveChanges() != 0;
    }

    internal bool SetAliasPassword(Alias alias, string password)
    {
        var hash = HashString(password);

        alias.Password = hash;
        _context.Aliases.Update(alias);

        return _context.SaveChanges() != 0;
    }

    internal bool RemoveAliasPassword(Alias alias)
    {
        alias.Password = null;
        _context.Aliases.Update(alias);

        return _context.SaveChanges() != 0;
    }

    internal string HashString(string value)
    {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(value));
        return System.Text.Encoding.UTF8.GetString(hashBytes);
    }
}
