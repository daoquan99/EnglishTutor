using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Speaking.Application.Shared.Errors;

public static class SpeakingErrors
{
    public static Error SessionNotFound(Guid id) =>
        Error.NotFound("Speaking session", id);

    public static Error TurnNotFound(Guid id) =>
        Error.NotFound("Speaking turn", id);

    public static readonly Error SessionNotActive =
        Error.Validation("Speaking session is not active.");

    public static readonly Error LanguageSettingsMissing =
        Error.Validation("User language settings are required to start a speaking session.");

    public static readonly Error TurnTextRequired =
        Error.Validation("User text is required until speech-to-text is enabled.");
}
