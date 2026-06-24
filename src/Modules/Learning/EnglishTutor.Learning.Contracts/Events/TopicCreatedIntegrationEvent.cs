using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Contracts.Events;

public sealed record TopicCreatedIntegrationEvent(
    Guid TopicId,
    string Name,
    string Slug) : IntegrationEvent;
