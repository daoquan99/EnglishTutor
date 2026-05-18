using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class UserDailyProgress : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public DateOnly Date { get; private set; }
    public int ExpEarned { get; private set; }
    public int ActivityCount { get; private set; }

    private UserDailyProgress() { }

    public static UserDailyProgress Create(Guid userId, LanguageCode targetLanguageCode, DateOnly date)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserDailyProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Date = date
        };
    }

    public void RecordActivity(int expEarned)
    {
        ExpEarned += Math.Max(0, expEarned);
        ActivityCount++;
    }
}
