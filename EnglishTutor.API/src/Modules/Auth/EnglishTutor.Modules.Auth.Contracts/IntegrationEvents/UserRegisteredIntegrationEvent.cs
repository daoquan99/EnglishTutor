using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;

public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime RegisteredAtUtc) : IntegrationEvent;
