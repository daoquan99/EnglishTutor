using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class UserStreak : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public int CurrentStreakDays { get; private set; }
    public int LongestStreakDays { get; private set; }
    public DateOnly? LastActivityDateUtc { get; private set; }

    private UserStreak() { }

    public static UserStreak Create(Guid userId, LanguageCode targetLanguageCode)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserStreak
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required.")
        };
    }

    public void RecordActivity(DateTime activityDateUtc)
    {
        var activityDate = DateOnly.FromDateTime(activityDateUtc);

        if (LastActivityDateUtc == activityDate)
        {
            return;
        }

        CurrentStreakDays = LastActivityDateUtc == activityDate.AddDays(-1)
            ? CurrentStreakDays + 1
            : 1;

        LongestStreakDays = Math.Max(LongestStreakDays, CurrentStreakDays);
        LastActivityDateUtc = activityDate;
    }
}
