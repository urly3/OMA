using OMA.Core.Data;
using OMA.Core.Models;
using OMA.Core.Models.Dto;
using OMA.Core.Models.Internal;

namespace OMA.Core.Services;

internal class OmaService
{
    private readonly OmaContext _context;

    public OmaService(OmaContext context)
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
        if (_context.GetAlias(aliasHash) != null) return null;

        return _context.CreateAlias(aliasHash);
    }

    public Alias? GetOrCreateAlias(string aliasHash)
    {
        var alias = _context.GetAlias(aliasHash);
        if (alias != null) return alias;

        return _context.CreateAlias(aliasHash);
    }

    public OmaStatus SetAliasPassword(AliasDto dto, string passwordHash)
    {
        var alias = GetAliasFromDto(dto);

        if (alias == null) return OmaStatus.AliasDoesNotExist;

        if (alias.Password != null) return OmaStatus.AliasIsLocked;

        return _context.SetAliasPassword(alias, passwordHash) ? OmaStatus.PasswordSet : OmaStatus.PasswordCouldNotBeSet;
    }

    public OmaStatus UnsetAliasPassword(AliasDto dto, string passwordHash)
    {
        var alias = GetAliasFromDto(dto);

        if (alias == null) return OmaStatus.AliasDoesNotExist;

        if (alias.Password == null) return OmaStatus.AliasIsUnlocked;

        if (alias.Password != passwordHash) return OmaStatus.PasswordDoesNotMatch;

        return _context.RemoveAliasPassword(alias) ? OmaStatus.PasswordSet : OmaStatus.PasswordCouldNotBeSet;
    }

    public OmaStatus AddLobbyToAlias(string aliasHash, long lobbyId, int bestOf, int warmups, string? lobbyName,
        bool createIfNull)
    {
        var alias = GetAlias(aliasHash);
        if (alias == null)
        {
            if (!createIfNull) return OmaStatus.AliasDoesNotExist;

            alias = CreateAlias(aliasHash);
            if (alias == null) return OmaStatus.AliasCouldNotBeCreated;
        }
        else
        {
            if (alias.Password != null) return OmaStatus.AliasIsLocked;
        }

        if (!ImportService.DoesLobbyExist(lobbyId)) return OmaStatus.LobbyNotValid;

        if (string.IsNullOrWhiteSpace(lobbyName))
        {
            var tempMatch = ImportService.GetMatch(lobbyId);
            if (tempMatch == null) return OmaStatus.LobbyNotValid;
            lobbyName = tempMatch.Name;
        }

        var existingLobby = _context.GetLobbyEqual(lobbyName, lobbyId, bestOf, warmups);

        if (existingLobby != null)
        {
            if (alias.Lobbies.Contains(existingLobby)) return OmaStatus.AliasContainsLobby;

            return _context.AddLobbyToAlias(alias, existingLobby)
                ? OmaStatus.LobbyAdded
                : OmaStatus.LobbyCouldNotBeAdded;
        }

        Lobby lobby = new()
        {
            LobbyId = lobbyId,
            BestOf = bestOf,
            Warmups = warmups,
            LobbyName = lobbyName
        };

        return _context.AddLobbyToAlias(alias, lobby) ? OmaStatus.LobbyAdded : OmaStatus.LobbyCouldNotBeAdded;
    }

    public OmaStatus RemoveLobbyFromAlias(AliasDto dto, long lobbyId, string lobbyName)
    {
        var alias = GetAliasFromDto(dto);
        if (alias == null) return OmaStatus.AliasDoesNotExist;

        if (alias.Password != null) return OmaStatus.AliasIsLocked;

        var lobby = alias.Lobbies.FirstOrDefault(l => l.LobbyId == lobbyId && l.LobbyName == lobbyName);

        if (lobby == null) return OmaStatus.AliasDoesNotContainLobby;

        return _context.RemoveLobbyFromAlias(alias, lobby) ? OmaStatus.LobbyRemoved : OmaStatus.LobbyCouldNotBeRemoved;
    }

    public Match? GetMatch(long lobby, int bestOf, int warmups)
    {
        try
        {
            var match = ImportService.GetMatch(lobby, bestOf, warmups);
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
        if (alias == null) return null;

        try
        {
            List<Match> matches = [];

            foreach (var lobby in alias.Lobbies)
                matches.Add(ImportService.GetMatch(lobby.LobbyId, lobby.BestOf, lobby.Warmups) ?? new Match());

            return matches;
        }
        catch
        {
            return [];
        }
    }
}