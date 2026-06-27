using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Quota.Domain.Aggregates.UsageLog;

/// <summary>
/// Represents a usage log entry.
/// </summary>
public class UsageLog : Entity
{
    public Guid UserId { get; private set; }
    public Guid QuotaReservationId { get; private set; }
    public Guid? PracticeSessionId { get; private set; }
    public int DurationMinutes { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string Source { get; private set; } = default!;

    private UsageLog() { }

    public static UsageLog Create(
        Guid id,
        Guid userId,
        Guid quotaReservationId,
        int durationMinutes,
        DateTime occurredAtUtc,
        string source,
        Guid? practiceSessionId = null)
    {
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source is required.", nameof(source));
        if (occurredAtUtc == DateTime.MinValue)
            throw new ArgumentException("OccurredAtUtc required.", nameof(occurredAtUtc));

        return new UsageLog
        {
            Id = id,
            UserId = userId,
            QuotaReservationId = quotaReservationId,
            PracticeSessionId = practiceSessionId,
            DurationMinutes = durationMinutes,
            OccurredAtUtc = occurredAtUtc,
            Source = source
        };
    }
}