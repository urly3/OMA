using Microsoft.Extensions.Caching.Memory;
using OMA.Core.Data;
using OMA.Core.Models;
using Imported = OMA.Core.Models.Imported;
using Internal = OMA.Core.Models.Internal;

namespace OMA.Core.Services;

internal class OmaService(OmaContext context, IMemoryCache memoryCache)
{
    private const int CacheExpirationInMinutes = 60;

    private static MemoryCacheEntryOptions CacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationInMinutes),
    };

    // data stuff //
    public OmaStatusResult<Alias> GetAlias(string aliasName)
    {
        var aliasHash = OmaUtil.HashString(aliasName);
        var alias = context.GetAlias(aliasHash);
        var status = alias == null ? OmaStatus.AliasDoesNotExist : OmaStatus.AliasExists;

        return new OmaStatusResult<Alias>(alias, status);
    }

    public OmaStatusResult<Alias> CreateAlias(string aliasName)
    {
        var aliasHash = OmaUtil.HashString(aliasName);

        var alias = context.CreateAlias(aliasHash);
        var status = alias == null ? OmaStatus.AliasCouldNotBeCreated : OmaStatus.AliasCreated;

        return new OmaStatusResult<Alias>(alias, status);
    }

    public OmaStatusResult<Lobby> GetLobby(string lobbyName, long lobbyId, int bestOf, int warmups)
    {
        var lobby = context.GetLobbyEqual(lobbyName, lobbyId, bestOf, warmups);
        var status = lobby == null ? OmaStatus.LobbyNotValid : OmaStatus.Ok;

        return new OmaStatusResult<Lobby>(lobby, status);
    }

    public OmaStatus AddLobbyToAlias(Alias alias, Lobby lobby)
    {
        var existingLobby = context.GetLobbyEqual(lobby.LobbyName, lobby.LobbyId, lobby.BestOf, lobby.Warmups);

        if (existingLobby != null)
            if (alias.Lobbies.Contains(existingLobby))
                return OmaStatus.AliasContainsLobby;

        return context.AddLobbyToAlias(alias, existingLobby ?? lobby)
            ? OmaStatus.LobbyAdded
            : OmaStatus.LobbyCouldNotBeAdded;
    }

    public OmaStatus RemoveLobbyFromAlias(Alias alias, Lobby lobby)
    {
        if (!alias.Lobbies.Contains(lobby))
            return OmaStatus.AliasDoesNotContainLobby;

        return context.RemoveLobbyFromAlias(alias, lobby)
            ? OmaStatus.LobbyRemoved
            : OmaStatus.LobbyCouldNotBeRemoved;
    }

    public OmaStatus LockAlias(Alias alias, string passwordHash)
    {
        if (alias.Password != null)
            return OmaStatus.AliasIsLocked;

        return context.SetAliasPassword(alias, passwordHash)
            ? OmaStatus.PasswordSet
            : OmaStatus.PasswordCouldNotBeSet;
    }

    public OmaStatus UnlockAlias(Alias alias, string passwordHash)
    {
        if (alias.Password == null)
            return OmaStatus.AliasIsUnlocked;

        return context.RemoveAliasPassword(alias)
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
        if (memoryCache.TryGetValue(lobbyId, out Imported.Lobby? cached))
        {
            memoryCache.Set(lobbyId, cached, CacheEntryOptions);
            return cached;
        }

        var lobby = ImportService.GetLobbyFromId(lobbyId);

        if (lobby != null)
            memoryCache.Set(lobbyId, lobby, CacheEntryOptions);

        return lobby;
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
        return memoryCache.TryGetValue(lobbyId, out _) || GetImportedLobbyFromId(lobbyId) != null;
    }
}
