using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.AdminReports.Application.Shared.Errors;

public static class AdminReportErrors
{
    public static Error DeadLetterNotFound(Guid messageId) =>
        Error.NotFound("Dead letter message", messageId);
}
