namespace OMA.Services;

enum OMAStatus {
    ActionFailed,

    AliasCrated,
    AliasDoesNotExist,
    AliasExists,
    AliasLocked,
    AliasUnlocked,
    InvalidPassword,

    LobbyAdded,
    LobbyRemoved,
    LobbyDoesNotExist,

    MatchBadResponse,
    MatchUrlInvalid,
    MatchTypeInvalid,
    MatchFailedToDeserialise,
    MatchHasNoUsers,
    MatchHasNoEvents,
}
