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

    public Alias? GetAlias(string name)
    {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(name));
        var hash = System.Text.Encoding.UTF8.GetString(hashBytes);

        return _context.Aliases.Where(a => a.Hash == hash).FirstOrDefault();
    }

    public bool CreateAlias(string name)
    {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(name));
        var hash = System.Text.Encoding.UTF8.GetString(hashBytes);

        _context.Aliases.Add(new()
        {
            Hash = hash,
        });

        return _context.SaveChanges() == 1;
    }

    public bool AddLobbyToAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Add(lobby);
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }

    public bool RemoveLobbyFromAlias(Alias alias, Lobby lobby)
    {
        alias.Lobbies.Remove(lobby);
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }

    public bool SetAliasPassword(Alias alias, string password)
    {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(password));
        var hash = System.Text.Encoding.UTF8.GetString(hashBytes);

        alias.Password = hash;
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }

    public bool RemoveAliasPassword(Alias alias)
    {
        alias.Password = null;
        _context.Aliases.Update(alias);

        return _context.SaveChanges() == 1;
    }
}
