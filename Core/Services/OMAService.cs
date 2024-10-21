using OMA.Core;
using OMA.Data;
using OMA.Models;
using OMA.Models.Dto;
using OMA.Models.Internal;

namespace OMA.Services;

class OMAService
{
    private OMAContext _context;

    public OMAService(OMAContext context)
    {
        _context = context;
    }

    public bool AliasExists(string aliasHash)
    {
        return _context.GetAlias(aliasHash) != null;
    }

    // this should only be called after AliasExists, so we know the
    // alias is not null.
    public bool AliasHasPassword(string aliasHash)
    {
        return _context.GetAlias(aliasHash)?.Password != null;
    }

    public OMAStatus AliasPasswordIsValid(string aliasHash, string passwordHash)
    {
        var alias = _context.GetAlias(aliasHash);
        if (alias == null)
        {
            return OMAStatus.AliasDoesNotExist;
        }

        return alias.Password == passwordHash ? OMAStatus.PasswordMatches : OMAStatus.PasswordDoesNotMatch;
    }

    public Alias? GetAlias(string aliasHash)
    {
        return _context.GetAlias(aliasHash);
    }

    public AliasDto? GetAliasAsDto(string aliasHash)
    {
        AliasDto dto = new(_context.GetAlias(aliasHash));

        return dto.Id == -1 ? null : dto;
    }

    public Alias? GetAliasFromDto(AliasDto dto)
    {
        return _context.GetAliasById(dto.Id);
    }

    public Alias? CreateAlias(string aliasHash)
    {
        if (_context.GetAlias(aliasHash) != null)
        {
            return null;
        }

        return _context.CreateAlias(aliasHash);
    }

    public Alias? GetOrCreateAlias(string aliasHash)
    {
        Alias? alias = _context.GetAlias(aliasHash);
        if (alias != null)
        {
            return alias;
        }

        return _context.CreateAlias(aliasHash);
    }

    public OMAStatus SetAliasPassword(string aliasHash, string passwordHash)
    {
        Alias? alias = GetAlias(aliasHash);

        if (alias == null)
        {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password != null)
        {
            return OMAStatus.AliasIsLocked;
        }

        return _context.SetAliasPassword(alias, passwordHash) ? OMAStatus.PasswordSet : OMAStatus.PasswordCouldNotBeSet;
    }

    public OMAStatus UnsetAliasPassword(string aliasHash)
    {
        Alias? alias = GetAlias(aliasHash);

        if (alias == null)
        {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password == null)
        {
            return OMAStatus.AliasIsUnlocked;
        }

        return _context.RemoveAliasPassword(alias) ? OMAStatus.PasswordSet : OMAStatus.PasswordCouldNotBeSet;
    }

    public OMAStatus AddLobbyToAlias(string aliasHash, long lobbyId, int bestOf, int warmups, string? lobbyName, bool createIfNull = false)
    {
        Alias? alias = GetAlias(aliasHash);
        if (alias == null)
        {
            if (!createIfNull)
            {
                return OMAStatus.AliasDoesNotExist;
            }

            alias = CreateAlias(aliasHash);
            if (alias == null)
            {
                return OMAStatus.AliasCouldNotBeCreated;
            }
        }
        else
        {
            if (alias.Password != null)
            {
                return OMAStatus.AliasIsLocked;
            }
        }

        var existingLobby = _context.GetLobbyEqual(lobbyId, bestOf, warmups);

        if (existingLobby != null)
        {
            if (alias.Lobbies.Contains(existingLobby))
            {
                return OMAStatus.AliasContainsLobby;
            }

            return _context.AddLobbyToAlias(alias, existingLobby) ? OMAStatus.LobbyAdded : OMAStatus.LobbyCouldNotBeAdded;
        }

        // we don't have that exact lobby stored in the database, start to make a new one.

        if (!OMAImportService.DoesLobbyExist(lobbyId))
        {
            return OMAStatus.LobbyDoesNotExist;
        }

        if (string.IsNullOrWhiteSpace(lobbyName))
        {
            lobbyName = OMAImportService.GetMatch(lobbyId).Name;
        }

        Lobby lobby = new()
        {
            LobbyId = lobbyId,
            BestOf = bestOf,
            Warmups = warmups,
            LobbyName = lobbyName
        };

        return _context.AddLobbyToAlias(alias, lobby) ? OMAStatus.LobbyAdded : OMAStatus.LobbyCouldNotBeAdded;
    }

    public OMAStatus RemoveLobbyFromAlias(string aliasHash, long lobbyId)
    {
        Alias? alias = GetAlias(aliasHash);
        if (alias == null)
        {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password != null)
        {
            return OMAStatus.AliasIsLocked;
        }

        var lobby = alias.Lobbies.FirstOrDefault(x => x.LobbyId == lobbyId);

        if (lobby == null)
        {
            return OMAStatus.AliasDoesNotContainLobby;
        }

        return _context.RemoveLobbyFromAlias(alias, lobby) ? OMAStatus.LobbyRemoved : OMAStatus.LobbyCouldNotBeRemoved;
    }

    public Match? GetMatch(long lobby, int bestOf, int warmups)
    {
        try
        {
            var match = OMAImportService.GetMatch(lobby, bestOf, warmups);
            return match;
        }
        catch
        {
            return null;
        }
    }

    public List<Match>? GetMatches(string aliasHash)
    {
        var alias = GetAliasAsDto(aliasHash);
        if (alias == null)
        {
            return null;
        }

        try
        {
            List<Match> matches = [];

            foreach (var lobby in alias.Lobbies)
            {
                matches.Add(OMAImportService.GetMatch(lobby.LobbyId, lobby.BestOf, lobby.Warmups));
            }

            return matches;
        }
        catch
        {
            return [];
        }
    }
}
