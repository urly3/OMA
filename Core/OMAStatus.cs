namespace OMA.Core;

internal enum OmaStatus
{
    ActionFailed,

    AliasCrated,
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