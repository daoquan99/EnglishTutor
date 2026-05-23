using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Assessments.Application.Shared.Errors;

public static class AssessmentErrors
{
    public static readonly Error DefinitionNotFound = Error.NotFound("Assessment definition was not found.");
    public static Error AttemptNotFound(Guid attemptId) => Error.NotFound("Assessment attempt", attemptId);
    public static readonly Error NotEligible = Error.Conflict("Learner is not eligible for this assessment yet.");
    public static readonly Error AttemptAlreadySubmitted = Error.Conflict("Assessment attempt has already been submitted.");
    public static readonly Error NotAllQuestionsAnswered = Error.Conflict("All assessment questions must be answered before submission.");
    public static readonly Error AttemptNotOwned = Error.Forbidden("Assessment attempt does not belong to the current user.");
    public static readonly Error AlreadyGraded = Error.Conflict("Assessment attempt has already been graded.");
    public static readonly Error ResultNotFound = Error.NotFound("Assessment attempt has not been graded yet.");
    public static readonly Error TargetLanguageNotFound = Error.NotFound("No active target language found for the current user.");
}
