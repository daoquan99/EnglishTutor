using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog.Enums;
using EnglishTutor.Modules.AdminReports.Domain.Shared;

namespace EnglishTutor.Modules.AdminReports.Application.Commands.CreateAuditLog;

public sealed class CreateAuditLogCommandHandler(
    IAdminAuditLogRepository auditLogRepository,
    IAdminReportsUnitOfWork unitOfWork) : ICommandHandler<CreateAuditLogCommand>
{
    public async Task<Result> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(
            AuditLog.Create(
                request.AdminUserId,
                ResolveAction(request.Action),
                request.TargetEntity,
                request.TargetEntityId,
                request.OldValue,
                request.NewValue,
                request.IpAddress,
                request.UserAgent),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static AuditAction ResolveAction(string action) =>
        Enum.TryParse<AuditAction>(action, ignoreCase: true, out var parsed)
            ? parsed
            : AuditAction.AdminCommandExecuted;
}
