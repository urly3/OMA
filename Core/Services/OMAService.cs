using OMA.Core.Data;
using OMA.Core.Models;
using OMA.Core.Models.Dto;
using OMA.Core.Models.Internal;

namespace OMA.Core.Services;

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

    public OMAStatus SetAliasPassword(AliasDto dto, string passwordHash)
    {
        Alias? alias = GetAliasFromDto(dto);

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

    public OMAStatus UnsetAliasPassword(AliasDto dto, string passwordHash)
    {
        Alias? alias = GetAliasFromDto(dto);

        if (alias == null)
        {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password == null)
        {
            return OMAStatus.AliasIsUnlocked;
        }

        if (alias.Password != passwordHash)
        {
            return OMAStatus.PasswordDoesNotMatch;
        }

        return _context.RemoveAliasPassword(alias) ? OMAStatus.PasswordSet : OMAStatus.PasswordCouldNotBeSet;
    }

    public OMAStatus AddLobbyToAlias(string aliasHash, long lobbyId, int bestOf, int warmups, string? lobbyName, bool createIfNull)
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

        if (!OMAImportService.DoesLobbyExist(lobbyId))
        {
            return OMAStatus.LobbyNotValid;
        }

        if (string.IsNullOrWhiteSpace(lobbyName))
        {
            var tempMatch = OMAImportService.GetMatch(lobbyId);
            if (tempMatch == null)
            {
                return OMAStatus.LobbyNotValid;
            }
            lobbyName = tempMatch.Name;
        }

        var existingLobby = _context.GetLobbyEqual(lobbyName, lobbyId, bestOf, warmups);

        if (existingLobby != null)
        {
            if (alias.Lobbies.Contains(existingLobby))
            {
                return OMAStatus.AliasContainsLobby;
            }

            return _context.AddLobbyToAlias(alias, existingLobby) ? OMAStatus.LobbyAdded : OMAStatus.LobbyCouldNotBeAdded;
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

    public OMAStatus RemoveLobbyFromAlias(AliasDto dto, long lobbyId, string lobbyName)
    {
        Alias? alias = GetAliasFromDto(dto);
        if (alias == null)
        {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password != null)
        {
            return OMAStatus.AliasIsLocked;
        }

        var lobby = alias.Lobbies.FirstOrDefault(l => l.LobbyId == lobbyId && l.LobbyName == lobbyName);

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
        catch (Exception e)
        {
            Console.WriteLine($"exception in get match: {e}");
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
                matches.Add(OMAImportService.GetMatch(lobby.LobbyId, lobby.BestOf, lobby.Warmups) ?? new() { });
            }

            return matches;
        }
        catch
        {
            return [];
        }
    }
}
