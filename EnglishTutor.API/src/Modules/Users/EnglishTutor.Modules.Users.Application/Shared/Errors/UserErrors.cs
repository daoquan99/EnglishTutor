using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Users.Application.Shared.Errors;

public static class UserErrors
{
    public static Error ProfileNotFound(Guid userId) =>
        Error.NotFound("User profile", userId);

    public static Error LanguageSettingsNotFound(Guid userId) =>
        Error.NotFound("User language settings", userId);

    public static Error TargetLanguageNotFound(Guid targetLanguageId) =>
        Error.NotFound("Target language", targetLanguageId);

    public static Error TargetLanguageNotFound(string targetLanguageCode) =>
        Error.NotFound("Target language", targetLanguageCode);

    public static readonly Error TargetLanguageAlreadyExists =
        Error.Conflict("Target language already exists for this user.");

    public static readonly Error CannotDeactivateActiveTargetLanguage =
        Error.Validation("The active target language cannot be deactivated without activating another target language.");
}
