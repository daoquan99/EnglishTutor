using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Exercises.Application.Shared.Errors;

public static class ExerciseErrors
{
    public static Error ExerciseSetNotFound(Guid id) =>
        Error.NotFound("Exercise set", id);

    public static Error AttemptNotFound(Guid id) =>
        Error.NotFound("Exercise attempt", id);

    public static Error ResultNotFound(Guid id) =>
        Error.NotFound("Exercise result", id);

    public static readonly Error ContentNotPublished =
        Error.Conflict("Exercise set is not published.");

    public static readonly Error AttemptAlreadyCompleted =
        Error.Conflict("Exercise attempt is already completed.");

    public static Error QuestionNotFound(Guid id) =>
        Error.NotFound("Exercise question", id);

    public static readonly Error QuestionAlreadyAnswered =
        Error.Conflict("Question has already been answered.");

    public static readonly Error NotAllQuestionsAnswered =
        Error.Conflict("All questions must be answered before completing the exercise.");

    public static readonly Error Forbidden =
        Error.Forbidden("You do not have access to this exercise attempt.");
}
