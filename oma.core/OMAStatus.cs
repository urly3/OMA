namespace OMA.Core;

enum OMAStatus {
    ActionFailed,

    AliasCrated,
    AliasDoesNotExist,
    AliasExists,
    AliasIsLocked,
    AliasIsUnlocked,
    AliasContainsLobby,
    AliasDoesNotContainLobby,
    InvalidPassword,
    PasswordSet,
    PasswordCouldNotBeSet,
    PasswordMatches,
    PasswordDoesNotMatch,

    LobbyAdded,
    LobbyCouldNotBeAdded,
    LobbyRemoved,
    LobbyCouldNotBeRemoved,
    LobbyDoesNotExist,

    MatchBadResponse,
    MatchUrlInvalid,
    MatchTypeInvalid,
    MatchFailedToDeserialise,
    MatchHasNoUsers,
    MatchHasNoEvents,
}
