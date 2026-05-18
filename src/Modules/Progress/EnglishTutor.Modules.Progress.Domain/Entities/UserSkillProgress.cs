using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class UserSkillProgress : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public LearningSkill Skill { get; private set; }
    public int Score { get; private set; }
    public int ActivityCount { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private UserSkillProgress() { }

    public static UserSkillProgress Create(Guid userId, LanguageCode targetLanguageCode, LearningSkill skill)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserSkillProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Skill = skill,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void RecordScore(int score)
    {
        var normalizedScore = Math.Clamp(score, 0, 100);
        Score = ActivityCount == 0
            ? normalizedScore
            : (int)Math.Round((Score * ActivityCount + normalizedScore) / (decimal)(ActivityCount + 1));
        ActivityCount++;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
