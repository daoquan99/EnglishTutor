using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;

/// <summary>
/// Represents a rate limit event.
/// </summary>
public class RateLimitEvent : Entity
{
    public enum LimitTypeEnum
    {
        DailyMinutesExceeded,
        DailySessionsExceeded,
        SingleSessionDurationExceeded
    }

    public Guid UserId { get; private set; }
    public string Reason { get; private set; } = default!;
    public LimitTypeEnum LimitType { get; private set; }
    public int AttemptedAmount { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public Guid? CorrelationId { get; private set; }

    private RateLimitEvent() { }

    public static RateLimitEvent Create(
        Guid id,
        Guid userId,
        string reason,
        LimitTypeEnum limitType,
        int attemptedAmount,
        DateTime occurredAtUtc,
        Guid? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));
        if (attemptedAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(attemptedAmount));
        if (occurredAtUtc == DateTime.MinValue)
            throw new ArgumentException("OccurredAtUtc required.", nameof(occurredAtUtc));

        return new RateLimitEvent
        {
            Id = id,
            UserId = userId,
            Reason = reason,
            LimitType = limitType,
            AttemptedAmount = attemptedAmount,
            OccurredAtUtc = occurredAtUtc,
            CorrelationId = correlationId
        };
    }
}