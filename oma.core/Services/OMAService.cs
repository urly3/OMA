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

    public bool ValidateAliasPassword(string name, string password) {
        var alias = _context.GetAlias(name);
        if (alias == null) {
            return false;
        }

        var passwordHash = _context.HashString(password);

        return alias.Password == passwordHash;
    }

    public Alias? GetAlias(string name) {
        return _context.GetAlias(name);
    }

    public AliasDto? GetAliasAsDto(string name) {
        AliasDto dto = new(_context.GetAlias(name));

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

    public bool SetAliasPassword(string name, string password) {
        Alias? alias = GetAlias(name);

        if (alias == null) {
            return false;
        }

        if (alias.Password != null) {
            return false;
        }

        return _context.SetAliasPassword(alias, password);
    }

    public bool UnsetAliasPassword(string name) {
        Alias? alias = GetAlias(name);

        if (alias == null) {
            return false;
        }

        if (alias.Password == null) {
            return false;
        }

        return _context.RemoveAliasPassword(alias);
    }

    public bool AddLobbyToAlias(string name, long lobbyId, int bestOf, int warmups) {
        Alias? alias = GetAlias(name);
        if (alias == null) {
            return false;
        }

        if (alias.Password != null) {
            return false;
        }

        var existingLobby = _context.GetLobbyEqual(lobbyId, bestOf, warmups);

        if (existingLobby != null) {
            return _context.AddLobbyToAlias(alias, existingLobby);
        }

        // we don't have that exact lobby stored in the database, start to make a new one.

        if (!OMAImportService.DoesLobbyExist(lobbyId)) {
            return false;
        }

        Lobby lobby = new() {
            LobbyId = lobbyId,
            BestOf = bestOf,
            Warmups = warmups,
        };

        return _context.AddLobbyToAlias(alias, lobby);
    }

    public bool RemoveLobbyFromAlias(string name, long lobbyId) {
        Alias? alias = GetAlias(name);
        if (alias == null) {
            return false;
        }

        if (alias.Password != null) {
            return false;
        }

        var lobby = alias.Lobbies.FirstOrDefault(x => x.LobbyId == lobbyId);

        return lobby != null ? _context.RemoveLobbyFromAlias(alias, lobby) : false;
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
}
