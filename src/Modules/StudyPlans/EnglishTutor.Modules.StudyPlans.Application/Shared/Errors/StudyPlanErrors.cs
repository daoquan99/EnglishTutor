using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;

public static class StudyPlanErrors
{
    public static readonly Error PlanAlreadyExists =
        Error.Conflict("An active study plan already exists for this target language.");

    public static readonly Error PlanNotFound =
        Error.NotFound("No active study plan found for the current user.");

    public static Error SessionNotFound(Guid sessionId) =>
        Error.NotFound("Planned study session", sessionId);

    public static readonly Error SessionNotOwned =
        Error.Forbidden("Planned study session does not belong to the current user.");

    public static readonly Error InvalidTimeZone =
        Error.Validation("Time zone id is invalid.");

    public static readonly Error InvalidDateRange =
        Error.Validation("From date must be before or equal to to date.");
}
