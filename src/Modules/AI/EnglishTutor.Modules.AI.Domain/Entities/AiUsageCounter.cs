using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class AiUsageCounter : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public AiTaskType TaskType { get; private set; }
    public DateOnly UsageDate { get; private set; }
    public int RequestCount { get; private set; }
    public int TotalTokens { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private AiUsageCounter() { }

    public static AiUsageCounter Create(Guid userId, AiTaskType taskType, DateOnly usageDate)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new AiUsageCounter
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskType = taskType,
            UsageDate = usageDate,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void RecordRequest(int tokenCount)
    {
        if (tokenCount < 0)
        {
            throw new DomainException("Token count cannot be negative.");
        }

        RequestCount++;
        TotalTokens += tokenCount;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
