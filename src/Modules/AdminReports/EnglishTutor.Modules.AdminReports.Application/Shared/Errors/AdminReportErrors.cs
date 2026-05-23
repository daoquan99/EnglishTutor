using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.AdminReports.Application.Shared.Errors;

public static class AdminReportErrors
{
    public static readonly Error DeadLetterNotFound = Error.NotFound("Dead letter message", "requested");
}
