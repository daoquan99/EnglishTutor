using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Progress.Application.Errors;

public static class ProgressErrors
{
    public static readonly Error DashboardSnapshotNotFound =
        Error.NotFound("Dashboard snapshot", "requested");

    public static readonly Error ExperienceNotFound =
        Error.NotFound("User experience", "requested");

    public static readonly Error ActivityLogNotFound =
        Error.NotFound("Learning activity log", "requested");
}
