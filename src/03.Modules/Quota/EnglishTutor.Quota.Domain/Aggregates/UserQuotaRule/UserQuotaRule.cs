using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;

/// <summary>
/// Represents a quota rule for a user.
/// </summary>
public class UserQuotaRule : AggregateRoot
{
    public Guid? UserId { get; private set; } // null for global default
    public int DailyMaxSessionMinutes { get; private set; }
    public int DailyMaxSessions { get; private set; }
    public int MaxSingleSessionDuration { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public bool IsActive { get; private set; }
    public long Version { get; private set; }

    private UserQuotaRule() { }

    public static UserQuotaRule Create(
        Guid id,
        Guid? userId,
        int dailyMaxSessionMinutes,
        int dailyMaxSessions,
        int maxSingleSessionDuration,
        DateTime effectiveDate,
        bool isActive)
    {
        if (dailyMaxSessionMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(dailyMaxSessionMinutes));
        if (dailyMaxSessions <= 0)
            throw new ArgumentOutOfRangeException(nameof(dailyMaxSessions));
        if (maxSingleSessionDuration <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSingleSessionDuration));
        if (effectiveDate == DateTime.MinValue)
            throw new ArgumentException("EffectiveDate required.", nameof(effectiveDate));

        return new UserQuotaRule
        {
            Id = id,
            UserId = userId,
            DailyMaxSessionMinutes = dailyMaxSessionMinutes,
            DailyMaxSessions = dailyMaxSessions,
            MaxSingleSessionDuration = maxSingleSessionDuration,
            EffectiveDate = effectiveDate,
            IsActive = isActive,
            Version = 1 // Initial version
        };
    }

    public void Update(
        int dailyMaxSessionMinutes,
        int dailyMaxSessions,
        int maxSingleSessionDuration,
        DateTime effectiveDate,
        bool isActive)
    {
        if (dailyMaxSessionMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(dailyMaxSessionMinutes));
        if (dailyMaxSessions <= 0)
            throw new ArgumentOutOfRangeException(nameof(dailyMaxSessions));
        if (maxSingleSessionDuration <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSingleSessionDuration));
        if (effectiveDate == DateTime.MinValue)
            throw new ArgumentException("EffectiveDate required.", nameof(effectiveDate));

        DailyMaxSessionMinutes = dailyMaxSessionMinutes;
        DailyMaxSessions = dailyMaxSessions;
        MaxSingleSessionDuration = maxSingleSessionDuration;
        EffectiveDate = effectiveDate;
        IsActive = isActive;
        Version++; // Increment version on update
    }

    public void Deactivate()
    {
        IsActive = false;
        Version++; // Increment version on deactivate
    }

    public void MarkDeleted(Guid? deletedByUserId = null)
    {
        base.MarkDeleted(deletedByUserId, DateTime.UtcNow);
        Version++; // Increment version on delete (soft delete)
    }
}