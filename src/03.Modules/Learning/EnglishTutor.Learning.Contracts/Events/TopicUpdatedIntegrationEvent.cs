using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record TopicUpdatedIntegrationEvent(
    Guid TopicId,
    string Name,
    string Slug,
    string Description) : IntegrationEvent;
