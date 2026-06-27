using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

/// <summary>
/// Raised when a refresh token is rotated (consumed and replaced with a new
/// token in the same family). The old token id and the new token id are
/// emitted so downstream modules can correlate the rotation.
/// </summary>
public sealed record RefreshTokenRotatedDomainEvent(
    Guid UserId,
    Guid SessionId,
    Guid OldTokenId,
    Guid NewTokenId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
