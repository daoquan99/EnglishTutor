using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class UserWeeklyProgress : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public int Year { get; private set; }
    public int WeekNumber { get; private set; }
    public int ExpEarned { get; private set; }
    public int ActivityCount { get; private set; }

    private UserWeeklyProgress() { }

    public static UserWeeklyProgress Create(Guid userId, LanguageCode targetLanguageCode, int year, int weekNumber)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        if (weekNumber is < 1 or > 53)
        {
            throw new DomainException("Week number must be between 1 and 53.");
        }

        return new UserWeeklyProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Year = year,
            WeekNumber = weekNumber
        };
    }

    public void RecordActivity(int expEarned)
    {
        ExpEarned += Math.Max(0, expEarned);
        ActivityCount++;
    }
}
