using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyReviewSession : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private VocabularyReviewSession() { }

    public static VocabularyReviewSession Start(Guid userId, LanguageCode targetLanguageCode, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new VocabularyReviewSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            StartedAtUtc = utcNow
        };
    }

    public void Complete(DateTime utcNow) => CompletedAtUtc ??= utcNow;
}
