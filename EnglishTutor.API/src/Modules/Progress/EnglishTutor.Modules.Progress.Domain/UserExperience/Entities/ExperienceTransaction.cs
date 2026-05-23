using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class ExperienceTransaction : Entity<Guid>
{
    public Guid UserExperienceId { get; private set; }
    public int Amount { get; private set; }
    public string SourceType { get; private set; } = string.Empty;
    public Guid SourceId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    private ExperienceTransaction() { }

    internal static ExperienceTransaction Create(
        Guid userExperienceId,
        int amount,
        string sourceType,
        Guid sourceId,
        string reason,
        DateTime utcNow)
    {
        if (userExperienceId == Guid.Empty || sourceId == Guid.Empty)
        {
            throw new DomainException("User experience id and source id are required.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Experience amount must be positive.");
        }

        return new ExperienceTransaction
        {
            Id = Guid.NewGuid(),
            UserExperienceId = userExperienceId,
            Amount = amount,
            SourceType = LearningActivityLog.Normalize(sourceType, 100, "Source type"),
            SourceId = sourceId,
            Reason = LearningActivityLog.Normalize(reason, 500, "Reason"),
            CreatedAtUtc = utcNow
        };
    }
}
