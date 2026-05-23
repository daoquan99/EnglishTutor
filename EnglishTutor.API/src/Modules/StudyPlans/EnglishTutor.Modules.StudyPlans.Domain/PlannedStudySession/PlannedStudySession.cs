using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;
using EnglishTutor.Modules.StudyPlans.Domain.Events;

namespace EnglishTutor.Modules.StudyPlans.Domain.Entities;

public sealed class PlannedStudySession : AggregateRoot<Guid>
{
    public Guid StudyPlanId { get; private set; }
    public Guid UserId { get; private set; }
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public DateTime ScheduledDateUtc { get; private set; }
    public PlannedSessionStatus Status { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? MissedAtUtc { get; private set; }

    private PlannedStudySession() { }

    public static PlannedStudySession Create(
        Guid studyPlanId,
        Guid userId,
        string targetLanguageCode,
        DateTime scheduledDateUtc,
        DateTime utcNow)
    {
        if (studyPlanId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Study plan id and user id are required.");
        }

        return new PlannedStudySession
        {
            Id = Guid.NewGuid(),
            StudyPlanId = studyPlanId,
            UserId = userId,
            TargetLanguageCode = NormalizeLanguageCode(targetLanguageCode),
            ScheduledDateUtc = DateTime.SpecifyKind(scheduledDateUtc, DateTimeKind.Utc),
            Status = PlannedSessionStatus.Planned,
            CreatedAtUtc = utcNow
        };
    }

    public void MarkMissed(DateTime utcNow)
    {
        EnsurePlanned();
        Status = PlannedSessionStatus.Missed;
        MissedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
        AddDomainEvent(new PlannedStudySessionMissedDomainEvent(
            UserId,
            Id,
            TargetLanguageCode,
            ScheduledDateUtc,
            utcNow));
    }

    public void Skip(DateTime utcNow)
    {
        EnsurePlanned();
        Status = PlannedSessionStatus.Skipped;
        UpdatedAtUtc = utcNow;
    }

    public void Complete(DateTime utcNow)
    {
        EnsurePlanned();
        Status = PlannedSessionStatus.Completed;
        CompletedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    private void EnsurePlanned()
    {
        if (Status != PlannedSessionStatus.Planned)
        {
            throw new DomainException("Planned study session is not in Planned status.");
        }
    }

    private static string NormalizeLanguageCode(string targetLanguageCode)
    {
        if (string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            throw new DomainException("Target language code is required.");
        }

        return targetLanguageCode.Trim().ToLowerInvariant();
    }
}
