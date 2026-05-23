using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Mistakes.Application.Shared.Errors;

public static class MistakeErrors
{
    public static Error MistakeNotFound(Guid id) =>
        Error.NotFound("Mistake", id);

    public static readonly Error MistakeAlreadyMastered =
        Error.Validation("Mistake has already been mastered.");
}
