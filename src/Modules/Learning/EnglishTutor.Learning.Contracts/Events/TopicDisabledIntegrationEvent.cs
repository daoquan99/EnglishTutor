using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record TopicDisabledIntegrationEvent(
    Guid TopicId) : IntegrationEvent;
