using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;

/// <summary>
/// Represents the quota state for a user on a specific date.
/// </summary>
public class UserQuotaState : AggregateRoot
{
    public Guid UserId { get; private set; }
    public DateTime QuotaDate { get; private set; } // Date only, stored as DateTime with time 00:00:00
    public int ReservedMinutes { get; private set; }
    public int UsedMinutes { get; private set; }
    public int ReservedSessionCount { get; private set; }
    public int UsedSessionCount { get; private set; }
    public long Version { get; private set; }

    private UserQuotaState() { }

    public static UserQuotaState Create(
        Guid id,
        Guid userId,
        DateTime quotaDate,
        int reservedMinutes,
        int usedMinutes,
        int reservedSessionCount,
        int usedSessionCount,
        long version)
    {
        if (quotaDate == DateTime.MinValue)
            throw new ArgumentException("QuotaDate required.", nameof(quotaDate));
        if (reservedMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(reservedMinutes));
        if (usedMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(usedMinutes));
        if (reservedSessionCount < 0)
            throw new ArgumentOutOfRangeException(nameof(reservedSessionCount));
        if (usedSessionCount < 0)
            throw new ArgumentOutOfRangeException(nameof(usedSessionCount));

        return new UserQuotaState
        {
            Id = id,
            UserId = userId,
            QuotaDate = quotaDate,
            ReservedMinutes = reservedMinutes,
            UsedMinutes = usedMinutes,
            ReservedSessionCount = reservedSessionCount,
            UsedSessionCount = usedSessionCount,
            Version = version
        };
    }

    public void UpdateQuotaDate(DateTime quotaDate)
    {
        if (quotaDate == DateTime.MinValue)
            throw new ArgumentException("QuotaDate required.", nameof(quotaDate));
        QuotaDate = quotaDate;
    }

    public void AddReservedMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes));
        ReservedMinutes += minutes;
    }

    public void ConsumeReservedMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes));
        if (ReservedMinutes < minutes)
            throw new InvalidOperationException("Not enough reserved minutes.");
        ReservedMinutes -= minutes;
    }

    public void AddUsedMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes));
        UsedMinutes += minutes;
    }

    public void ConsumeUsedMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes));
        if (UsedMinutes < minutes)
            throw new InvalidOperationException("Not enough used minutes.");
        UsedMinutes -= minutes;
    }

    public void IncrementReservedSessionCount()
    {
        ReservedSessionCount++;
    }

    public void DecrementReservedSessionCount()
    {
        if (ReservedSessionCount <= 0)
            throw new InvalidOperationException("Reserved session count cannot be negative.");
        ReservedSessionCount--;
    }

    public void IncrementUsedSessionCount()
    {
        UsedSessionCount++;
    }

    public void DecrementUsedSessionCount()
    {
        if (UsedSessionCount <= 0)
            throw new InvalidOperationException("Used session count cannot be negative.");
        UsedSessionCount--;
    }
}