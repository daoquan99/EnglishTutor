using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

namespace EnglishTutor.Modules.AdminReports.Application.EventHandlers;

public sealed class SpeakingSessionCompletedEventHandler(
    IAdminReportProjectionRepository projectionRepository,
    IAdminReportsInboxStore inboxStore,
    IAdminReportsUnitOfWork unitOfWork)
    : IIntegrationEventHandler<SpeakingSessionCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(SpeakingSessionCompletedEventHandler);

    public async Task HandleAsync(SpeakingSessionCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var card = await GetOrCreateCardAsync(@event.UserId, @event.CompletedAtUtc, ct);
        card.RecordSpeakingSession(@event.CompletedAtUtc);

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
