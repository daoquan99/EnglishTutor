using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Domain.Enums;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class LearningActivityLog : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public ActivityType ActivityType { get; private set; }
    public Guid ActivityId { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime CompletedAtUtc { get; private set; }
    public int DurationSeconds { get; private set; }
    public int ExpEarned { get; private set; }
    public int Score { get; private set; }
    public string Result { get; private set; } = string.Empty;

    private LearningActivityLog() { }

    public static LearningActivityLog Create(
        Guid userId,
        LanguageCode targetLanguageCode,
        ActivityType activityType,
        Guid activityId,
        DateTime startedAtUtc,
        DateTime completedAtUtc,
        int expEarned,
        int score,
        string result)
    {
        if (userId == Guid.Empty || activityId == Guid.Empty)
        {
            throw new DomainException("User id and activity id are required.");
        }

        if (completedAtUtc < startedAtUtc)
        {
            throw new DomainException("Completed time cannot be earlier than started time.");
        }

        return new LearningActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            ActivityType = activityType,
            ActivityId = activityId,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            DurationSeconds = Math.Max(0, (int)(completedAtUtc - startedAtUtc).TotalSeconds),
            ExpEarned = Math.Max(0, expEarned),
            Score = Math.Clamp(score, 0, 100),
            Result = Normalize(result, 200, "Result")
        };
    }

    internal static string Normalize(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
