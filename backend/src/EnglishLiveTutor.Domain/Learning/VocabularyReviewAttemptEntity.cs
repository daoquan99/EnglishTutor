using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Learning;

public sealed class VocabularyReviewAttemptEntity : AggregateRoot<Guid>
{
    public VocabularyReviewAttemptEntity() { Id = Guid.NewGuid(); }

    public string UserId { get; set; } = "default-learner";
    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }
    public string ReviewType { get; set; } = "FlashcardRecall";
    public string Rating { get; set; } = "Good"; // Again | Hard | Good | Easy
    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? NextReviewAtSnapshot { get; set; }
}
