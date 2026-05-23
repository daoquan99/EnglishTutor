using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class UserMonthlyProgress : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public int Year { get; private set; }
    public int Month { get; private set; }
    public int ExpEarned { get; private set; }
    public int ActivityCount { get; private set; }

    private UserMonthlyProgress() { }

    public static UserMonthlyProgress Create(Guid userId, LanguageCode targetLanguageCode, int year, int month)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        if (month is < 1 or > 12)
        {
            throw new DomainException("Month must be between 1 and 12.");
        }

        return new UserMonthlyProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Year = year,
            Month = month
        };
    }

    public void RecordActivity(int expEarned)
    {
        ExpEarned += Math.Max(0, expEarned);
        ActivityCount++;
    }
}
