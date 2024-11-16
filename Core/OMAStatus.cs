namespace OMA.Core;

public enum OmaStatus
{
    Ok,
    ActionFailed,

    AliasCreated,
    AliasDoesNotExist,
    AliasExists,
    AliasIsLocked,
    AliasIsUnlocked,
    AliasContainsLobby,
    AliasDoesNotContainLobby,
    AliasCouldNotBeCreated,
    InvalidPassword,
    PasswordSet,
    PasswordCouldNotBeSet,
    PasswordMatches,
    PasswordDoesNotMatch,

    LobbyAdded,
    LobbyCouldNotBeAdded,
    LobbyRemoved,
    LobbyCouldNotBeRemoved,
    LobbyNotValid,

    MatchBadResponse,
    MatchUrlInvalid,
    MatchTypeInvalid,
    MatchFailedToDeserialise,
    MatchHasNoUsers,
    MatchHasNoEvents
}