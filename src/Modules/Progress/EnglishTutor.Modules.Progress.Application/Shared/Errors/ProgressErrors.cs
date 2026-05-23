using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Progress.Application.Shared.Errors;

public static class ProgressErrors
{
    public static readonly Error DashboardSnapshotNotFound =
        Error.NotFound("Dashboard snapshot was not found for the current user.");

    public static readonly Error ExperienceNotFound =
        Error.NotFound("User experience record was not found.");

    public static readonly Error ActivityLogNotFound =
        Error.NotFound("Learning activity log was not found.");
}
