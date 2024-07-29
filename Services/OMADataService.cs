using OMA.Models;
using System.IO.Hashing;

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

        return _context.Aliases.Where(a => a.Hash == hash).FirstOrDefault();
    }

    internal Alias? GetAliasById(int id)
    {
        return _context.Aliases.FirstOrDefault(a => a.Id == id);
    }

    internal bool CreateAlias(string name)
    {
        var hash = HashString(name);

        _context.Aliases.Add(new()
        {
            Hash = hash,
        });

        return _context.SaveChanges() == 1;
    }

    internal bool AddLobbyToAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Add(lobby);
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }

    internal bool RemoveLobbyFromAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Remove(lobby);
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }

    internal bool SetAliasPassword(Alias alias, string password)
    {
        var hash = HashString(password);

        alias.Password = hash;
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }

    internal bool RemoveAliasPassword(Alias alias)
    {
        alias.Password = null;
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }

    internal string HashString(string value)
    {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(value));
        return System.Text.Encoding.UTF8.GetString(hashBytes);
    }
}
