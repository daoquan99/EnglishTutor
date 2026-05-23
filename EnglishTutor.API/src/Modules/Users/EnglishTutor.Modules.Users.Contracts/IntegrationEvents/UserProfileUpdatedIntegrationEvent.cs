using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Users.Contracts.IntegrationEvents;

public sealed record UserProfileUpdatedIntegrationEvent(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    DateTime UpdatedAtUtc) : IntegrationEvent;
