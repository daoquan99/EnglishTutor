using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Vocabulary.Application.Shared.Errors;

public static class VocabularyErrors
{
    public static Error VocabularyItemNotFound(Guid id) =>
        Error.NotFound("Vocabulary item", id);

    public static Error VocabularyExampleNotFound(Guid id) =>
        Error.NotFound("Vocabulary example", id);

    public static readonly Error MasteryNotFound =
        Error.NotFound("User vocabulary mastery", "requested");

    public static readonly Error ReviewScoreInvalid =
        Error.Validation("Review score must be between 0 and 100.");

    public static readonly Error TargetLanguageMismatch =
        Error.Validation("Vocabulary item does not belong to the requested target language.");
}
