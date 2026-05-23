using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;

namespace EnglishTutor.Modules.StudyPlans.Domain.Entities;

public sealed class StudyPlanTarget : Entity<Guid>
{
    public Guid StudyPlanId { get; private set; }
    public TargetPeriod Period { get; private set; }
    public int TargetMinutes { get; private set; }
    public int TargetStudyDays { get; private set; }

    private StudyPlanTarget() { }

    public static StudyPlanTarget Create(Guid studyPlanId, TargetPeriod period, int targetMinutes, int targetStudyDays, DateTime utcNow)
    {
        if (studyPlanId == Guid.Empty)
        {
            throw new DomainException("Study plan id is required.");
        }

        if (targetMinutes <= 0)
        {
            throw new DomainException("Target minutes must be positive.");
        }

        if (targetStudyDays < 0)
        {
            throw new DomainException("Target study days cannot be negative.");
        }

        return new StudyPlanTarget
        {
            Id = Guid.NewGuid(),
            StudyPlanId = studyPlanId,
            Period = period,
            TargetMinutes = targetMinutes,
            TargetStudyDays = targetStudyDays,
            CreatedAtUtc = utcNow
        };
    }

    public void Update(int targetMinutes, int targetStudyDays, DateTime utcNow)
    {
        if (targetMinutes <= 0)
        {
            throw new DomainException("Target minutes must be positive.");
        }

        if (targetStudyDays < 0)
        {
            throw new DomainException("Target study days cannot be negative.");
        }

        TargetMinutes = targetMinutes;
        TargetStudyDays = targetStudyDays;
        UpdatedAtUtc = utcNow;
    }
}
