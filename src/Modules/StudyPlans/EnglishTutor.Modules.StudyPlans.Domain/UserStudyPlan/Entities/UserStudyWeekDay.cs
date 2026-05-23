using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.StudyPlans.Domain.Entities;

public sealed class UserStudyWeekDay : Entity<Guid>
{
    public Guid StudyPlanId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsStudyDay { get; private set; }

    private UserStudyWeekDay() { }

    public static UserStudyWeekDay Create(Guid studyPlanId, DayOfWeek dayOfWeek, bool isStudyDay, DateTime utcNow)
    {
        if (studyPlanId == Guid.Empty)
        {
            throw new DomainException("Study plan id is required.");
        }

        return new UserStudyWeekDay
        {
            Id = Guid.NewGuid(),
            StudyPlanId = studyPlanId,
            DayOfWeek = dayOfWeek,
            IsStudyDay = isStudyDay,
            CreatedAtUtc = utcNow
        };
    }

    public void SetStudyDay(bool isStudyDay, DateTime utcNow)
    {
        IsStudyDay = isStudyDay;
        UpdatedAtUtc = utcNow;
    }
}
