using System.Collections.Concurrent;
using OMA.Core.Data;
using OMA.Core.Models;
using Imported = OMA.Core.Models.Imported;
using Internal = OMA.Core.Models.Internal;

namespace OMA.Core.Services;

internal class OmaService
{
    private const int CacheExpirationInMinutes = 60;
    private static readonly ConcurrentDictionary<long, CachedLobby> ImportedLobbiesCache = [];
    private static bool _firstRun = true;
    private readonly OmaContext _context;

    public OmaService(OmaContext context)
    {
        _context = context;
        if (_firstRun)
        {
            _ = Task.Run(CleanupCache).ConfigureAwait(false);
            _firstRun = false;
        }
    }

    private static async void CleanupCache()
    {
        try
        {
            while (true)
            {
                await Task.Delay(1000 * 60 * CacheExpirationInMinutes);

                foreach (var match in ImportedLobbiesCache)
                    if (match.Value.UpdatedAt < DateTime.UtcNow.AddMinutes(-CacheExpirationInMinutes))
                        ImportedLobbiesCache.Remove(match.Key, out var _);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // ReSharper disable once AsyncVoidMethod
            throw;
        }
    }

    // data stuff //
    public OmaStatusResult<Alias> GetAlias(string aliasName)
    {
        var aliasHash = OmaUtil.HashString(aliasName);
        var alias = _context.GetAlias(aliasHash);
        var status = alias == null ? OmaStatus.AliasDoesNotExist : OmaStatus.AliasExists;

        return new OmaStatusResult<Alias>(alias, status);
    }

    public OmaStatusResult<Alias> CreateAlias(string aliasName)
    {
        var aliasHash = OmaUtil.HashString(aliasName);

        var alias = _context.CreateAlias(aliasHash);
        var status = alias == null ? OmaStatus.AliasCouldNotBeCreated : OmaStatus.AliasCreated;

        return new OmaStatusResult<Alias>(alias, status);
    }

    public OmaStatusResult<Lobby> GetLobby(string lobbyName, long lobbyId, int bestOf, int warmups)
    {
        var lobby = _context.GetLobbyEqual(lobbyName, lobbyId, bestOf, warmups);
        var status = lobby == null ? OmaStatus.LobbyNotValid : OmaStatus.Ok;

        return new OmaStatusResult<Lobby>(lobby, status);
    }

    public OmaStatus AddLobbyToAlias(Alias alias, Lobby lobby)
    {
        var existingLobby = _context.GetLobbyEqual(lobby.LobbyName, lobby.LobbyId, lobby.BestOf, lobby.Warmups);

        if (existingLobby != null)
            if (alias.Lobbies.Contains(existingLobby))
                return OmaStatus.AliasContainsLobby;

        return _context.AddLobbyToAlias(alias, existingLobby ?? lobby)
            ? OmaStatus.LobbyAdded
            : OmaStatus.LobbyCouldNotBeAdded;
    }

    public OmaStatus RemoveLobbyFromAlias(Alias alias, Lobby lobby)
    {
        if (!alias.Lobbies.Contains(lobby))
            return OmaStatus.AliasDoesNotContainLobby;

        return _context.RemoveLobbyFromAlias(alias, lobby)
            ? OmaStatus.LobbyRemoved
            : OmaStatus.LobbyCouldNotBeRemoved;
    }

    public OmaStatus LockAlias(Alias alias, string passwordHash)
    {
        if (alias.Password != null)
            return OmaStatus.AliasIsLocked;

        return _context.SetAliasPassword(alias, passwordHash)
            ? OmaStatus.PasswordSet
            : OmaStatus.PasswordCouldNotBeSet;
    }

    public OmaStatus UnlockAlias(Alias alias, string passwordHash)
    {
        if (alias.Password == null)
            return OmaStatus.AliasIsUnlocked;

        return _context.RemoveAliasPassword(alias)
            ? OmaStatus.PasswordSet
            : OmaStatus.PasswordCouldNotBeSet;
    }

    // match stuff //
    public OmaStatusResult<Internal.Match> GetMatch(long lobbyId, int bestOf, int warmups)
    {
        var lobby = GetImportedLobbyFromId(lobbyId);
        if (lobby == null)
            return new OmaStatusResult<Internal.Match>(null, OmaStatus.MatchBadResponse);

        var match = ImportService.GetMatchFromLobby(lobby, bestOf, warmups);

        return new OmaStatusResult<Internal.Match>(match, OmaStatus.Ok);
    }

    // utility stuff //
    private Imported.Lobby? GetImportedLobbyFromId(long lobbyId)
    {
        if (ImportedLobbiesCache.TryGetValue(lobbyId, out var cached))
        {
            cached.UpdatedAt = DateTime.Now;
            return cached.Lobby;
        }

        var lobby = ImportService.GetLobbyFromId(lobbyId);

        if (lobby != null)
            ImportedLobbiesCache.TryAdd(lobbyId, new CachedLobby(lobby));

        return lobby ?? null;
    }

    public OmaStatusResult<string> GetLobbyNameFromLobbyId(long lobbyId)
    {
        var lobby = GetImportedLobbyFromId(lobbyId);

        return lobby == null
            ? new OmaStatusResult<string>(null, OmaStatus.LobbyNotValid)
            : new OmaStatusResult<string>(lobby.match!.name, OmaStatus.Ok);
    }

    public bool IsValidLobbyId(long lobbyId)
    {
        return ImportedLobbiesCache.ContainsKey(lobbyId) || GetImportedLobbyFromId(lobbyId) != null;
    }

    private struct CachedLobby(Imported.Lobby lobby)
    {
        public Imported.Lobby Lobby { get; } = lobby;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
