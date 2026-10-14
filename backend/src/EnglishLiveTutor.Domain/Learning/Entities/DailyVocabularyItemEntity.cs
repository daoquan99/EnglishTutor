using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Learning;

public sealed class DailyVocabularyItemEntity : Entity<Guid>
{
    public DailyVocabularyItemEntity() { Id = Guid.NewGuid(); }

    public Guid PlanId { get; set; }
    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }
    public string ItemType { get; set; } = "New"; // New | Review | CarryOver
    public string Status { get; set; } = "Pending"; // Pending | InProgress | Completed
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }

    public void Complete(DateTimeOffset now)
    {
        Status = "Completed";
        CompletedAt = now;
    }
}
