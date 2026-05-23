using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Application.Shared.Errors;

namespace EnglishTutor.Modules.AdminReports.Application.Commands.ReprocessDeadLetter;

public sealed class ReprocessDeadLetterCommandHandler(IAdminReportQueryService queryService)
    : ICommandHandler<ReprocessDeadLetterCommand>
{
    public async Task<Result> Handle(ReprocessDeadLetterCommand request, CancellationToken cancellationToken)
    {
        var reprocessed = await queryService.ReprocessDeadLetterAsync(request.DeadLetterId, cancellationToken);
        return reprocessed ? Result.Success() : Result.Failure(AdminReportErrors.DeadLetterNotFound);
    }
}
