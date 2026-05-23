using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.AdminReports.Application.EventHandlers;

public sealed class UserRegisteredEventHandler(
    IAdminReportProjectionRepository projectionRepository,
    IAdminReportsInboxStore inboxStore,
    IAdminReportsUnitOfWork unitOfWork)
    : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private const string HandlerName = nameof(UserRegisteredEventHandler);

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        if (await projectionRepository.GetUserOverviewCardAsync(@event.UserId, ct) is null)
        {
            await projectionRepository.AddUserOverviewCardAsync(
                UserOverviewCard.Create(@event.UserId, @event.Email, @event.DisplayName, @event.RegisteredAtUtc),
                ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
