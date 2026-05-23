using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.AdminReports.Contracts.IntegrationEvents;

public sealed record ProgressSummaryReadyIntegrationEvent(
    Guid UserId,
    string Period,
    DateOnly StartDate,
    DateOnly EndDate,
    int ActivityCount,
    int ExpEarned,
    DateTime GeneratedAtUtc) : IntegrationEvent;
