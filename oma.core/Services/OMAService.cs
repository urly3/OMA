using OMA.Core;
using OMA.Data;
using OMA.Models;
using OMA.Models.Dto;
using OMA.Models.Internal;

namespace OMA.Services;

class OMAService {
    private OMAContext _context;

    public OMAService(OMAContext context) {
        _context = context;
    }

    public bool AliasExists(string name) {
        return _context.GetAlias(name) != null;
    }

    // this should only be called after AliasExists, so we know the
    // alias is not null.
    public bool AliasHasPassword(string name) {
        return _context.GetAlias(name)?.Password != null;
    }

    public OMAStatus AliasPasswordIsValid(string name, string password) {
        var alias = _context.GetAlias(name);
        if (alias == null) {
            return OMAStatus.AliasDoesNotExist;
        }

        var passwordHash = _context.HashString(password);

        return alias.Password == passwordHash ? OMAStatus.PasswordMatches : OMAStatus.PasswordDoesNotMatch;
    }

    public Alias? GetAlias(string name) {
        return _context.GetAlias(name);
    }

    public AliasDto? GetAliasAsDto(string name) {
        AliasDto dto = new(_context.GetAlias(name));

        return dto.Id == -1 ? null : dto;
    }

    public AliasDto? GetAliasAsDtoFromHash(string hash) {
        AliasDto dto = new(_context.GetAliasFromHash(hash));

        return dto.Id == -1 ? null : dto;
    }

    public Alias? GetAliasFromDto(AliasDto dto) {
        return _context.GetAliasById(dto.Id);
    }

    public Alias? CreateAlias(string name) {
        if (_context.GetAlias(name) != null) {
            return null;
        }

        if (!_context.CreateAlias(name)) {
            return null;
        }

        return _context.GetAlias(name);
    }

    public Alias? GetOrCreateAlias(string name) {
        Alias? alias = _context.GetAlias(name);
        if (alias == null) {
            _ = _context.CreateAlias(name);
            alias = _context.GetAlias(name);
        }

        return alias;
    }

    public OMAStatus SetAliasPassword(string name, string password) {
        Alias? alias = GetAlias(name);

        if (alias == null) {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password != null) {
            return OMAStatus.AliasIsLocked;
        }

        return _context.SetAliasPassword(alias, password) ? OMAStatus.PasswordSet : OMAStatus.PasswordCouldNotBeSet;
    }

    public OMAStatus UnsetAliasPassword(string name) {
        Alias? alias = GetAlias(name);

        if (alias == null) {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password == null) {
            return OMAStatus.AliasIsUnlocked;
        }

        return _context.RemoveAliasPassword(alias) ? OMAStatus.PasswordSet : OMAStatus.PasswordCouldNotBeSet;
    }

    public OMAStatus AddLobbyToAlias(string name, long lobbyId, int bestOf, int warmups) {
        Alias? alias = GetAlias(name);
        if (alias == null) {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password != null) {
            return OMAStatus.AliasIsLocked;
        }

        var existingLobby = _context.GetLobbyEqual(lobbyId, bestOf, warmups);

        if (existingLobby != null) {
            if (alias.Lobbies.Contains(existingLobby)) {
                return OMAStatus.AliasContainsLobby;
            }
            return _context.AddLobbyToAlias(alias, existingLobby) ? OMAStatus.LobbyAdded : OMAStatus.LobbyCouldNotBeAdded;
        }

        // we don't have that exact lobby stored in the database, start to make a new one.

        if (!OMAImportService.DoesLobbyExist(lobbyId)) {
            return OMAStatus.LobbyDoesNotExist;
        }

        Lobby lobby = new() {
            LobbyId = lobbyId,
            BestOf = bestOf,
            Warmups = warmups,
        };

        return _context.AddLobbyToAlias(alias, lobby) ? OMAStatus.LobbyAdded : OMAStatus.LobbyCouldNotBeAdded;
    }

    public OMAStatus RemoveLobbyFromAlias(string name, long lobbyId) {
        Alias? alias = GetAlias(name);
        if (alias == null) {
            return OMAStatus.AliasDoesNotExist;
        }

        if (alias.Password != null) {
            return OMAStatus.AliasIsLocked;
        }

        var lobby = alias.Lobbies.FirstOrDefault(x => x.LobbyId == lobbyId);

        if (lobby == null) {
            return OMAStatus.AliasDoesNotContainLobby;
        }

        return _context.RemoveLobbyFromAlias(alias, lobby) ? OMAStatus.LobbyRemoved : OMAStatus.LobbyCouldNotBeRemoved;
    }

    public Match? GetMatch(long lobby, int bestOf, int warmups) {
        try {
            var match = OMAImportService.GetMatch(lobby, bestOf, warmups);
            return match;
        }
        catch {
            return null;
        }
    }

    public List<Match>? GetMatches(string name) {
        var alias = GetAliasAsDto(name);
        if (alias == null) {
            return null;
        }

        try {
            List<Match> matches = [];

            foreach (var lobby in alias.Lobbies) {
                matches.Add(OMAImportService.GetMatch(lobby.LobbyId, lobby.BestOf, lobby.Warmups));
            }

            return matches;
        }
        catch {
            return [];
        }
    }

    public string HashString(string alias) {
        return _context.HashString(alias);
    }
}
