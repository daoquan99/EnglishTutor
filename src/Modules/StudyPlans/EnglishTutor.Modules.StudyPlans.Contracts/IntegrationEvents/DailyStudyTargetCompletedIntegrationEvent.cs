using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;

public sealed record DailyStudyTargetCompletedIntegrationEvent(
    Guid UserId,
    string TargetLanguageCode,
    int ActualMinutes,
    int TargetMinutes,
    DateTime CompletedDateUtc) : IntegrationEvent;
