using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.AdminReports.Application.EventHandlers;

public sealed class UserLevelChangedEventHandler(
    IAdminReportProjectionRepository projectionRepository,
    IAdminReportsInboxStore inboxStore,
    IAdminReportsUnitOfWork unitOfWork)
    : IIntegrationEventHandler<UserLevelChangedIntegrationEvent>
{
    private const string HandlerName = nameof(UserLevelChangedEventHandler);

    public async Task HandleAsync(UserLevelChangedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var card = await GetOrCreateCardAsync(@event.UserId, @event.ChangedAtUtc, ct);
        card.UpdateLevel(@event.TargetLanguageCode, @event.NewLevel, @event.ChangedAtUtc);

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<UserOverviewCard> GetOrCreateCardAsync(Guid userId, DateTime utcNow, CancellationToken ct)
    {
        var card = await projectionRepository.GetUserOverviewCardAsync(userId, ct);
        if (card is not null)
        {
            return card;
        }

        card = UserOverviewCard.Create(userId, string.Empty, string.Empty, utcNow);
        await projectionRepository.AddUserOverviewCardAsync(card, ct);
        return card;
    }
}
