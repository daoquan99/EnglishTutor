using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyReviewAttempt : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid VocabularyItemId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public bool IsCorrect { get; private set; }
    public int Score { get; private set; }
    public DateTime ReviewedAtUtc { get; private set; }

    private VocabularyReviewAttempt() { }

    public static VocabularyReviewAttempt Create(
        Guid userId,
        Guid vocabularyItemId,
        LanguageCode targetLanguageCode,
        bool isCorrect,
        int score,
        DateTime utcNow)
    {
        if (userId == Guid.Empty || vocabularyItemId == Guid.Empty)
        {
            throw new DomainException("User id and vocabulary item id are required.");
        }

        return new VocabularyReviewAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VocabularyItemId = vocabularyItemId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            IsCorrect = isCorrect,
            Score = Math.Clamp(score, 0, 100),
            ReviewedAtUtc = utcNow
        };
    }
}
