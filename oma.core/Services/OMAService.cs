using OMA.Data;
using OMA.Models;
using OMA.Models.Dto;
using OMA.Models.Internal;

namespace OMA.Services;

class OMAService {
    private OMADataService _dataService;

    public OMAService(OMADataService dataService) {
        _dataService = dataService;
    }

    public bool AliasExists(string name) {
        return _dataService.GetAlias(name) != null;
    }

    public bool AliasHasPassword(string name) {
        // this should only be called after AliasExists, so we know the
        // alias is not null.
        return _dataService.GetAlias(name)?.Password != null;
    }

    public bool ValidateAliasPassword(string name, string password) {
        var alias = _dataService.GetAlias(name);
        if (alias == null) {
            return false;
        }

        var passwordHash = _dataService.HashString(password);

        return alias.Password == passwordHash;
    }

    public Alias? GetAlias(string name) {
        return _dataService.GetAlias(name);
    }

    public AliasDto? GetAliasAsDto(string name) {
        AliasDto dto = new(_dataService.GetAlias(name));

        return dto.Id == -1 ? null : dto;
    }

    public Alias? GetAliasFromDto(AliasDto dto) {
        return _dataService.GetAliasById(dto.Id);
    }

    public Alias? CreateAlias(string name) {
        if (_dataService.GetAlias(name) != null) {
            return null;
        }

        if (!_dataService.CreateAlias(name)) {
            return null;
        }

        return _dataService.GetAlias(name);
    }

    public Alias? GetOrCreateAlias(string name) {
        Alias? alias = _dataService.GetAlias(name);
        if (alias == null) {
            _ = _dataService.CreateAlias(name);
            alias = _dataService.GetAlias(name);
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

        return _dataService.SetAliasPassword(alias, password);
    }

    public bool UnsetAliasPassword(string name) {
        Alias? alias = GetAlias(name);

        if (alias == null) {
            return false;
        }

        if (alias.Password == null) {
            return false;
        }

        return _dataService.RemoveAliasPassword(alias);
    }

    public bool AddLobbyToAlias(string name, long lobbyId, int bestOf, int warmups) {
        Alias? alias = GetAlias(name);
        if (alias == null) {
            return false;
        }

        if (alias.Password != null) {
            return false;
        }

        if (!OMAImportService.DoesLobbyExist(lobbyId)) {
            return false;
        }

        Lobby lobby = new() {
            LobbyId = lobbyId,
            BestOf = bestOf,
            Warmups = warmups,
        };

        return _dataService.AddLobbyToAlias(alias, lobby);
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

        return lobby != null ? _dataService.RemoveLobbyFromAlias(alias, lobby) : false;
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
