using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class ExampleFillBlankAttempt : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid VocabularyItemId { get; private set; }
    public Guid VocabularyExampleId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public string UserAnswer { get; private set; } = string.Empty;
    public string CorrectAnswer { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int Score { get; private set; }
    public DateTime AttemptedAtUtc { get; private set; }

    private ExampleFillBlankAttempt() { }

    public static ExampleFillBlankAttempt Create(
        Guid userId,
        Guid vocabularyItemId,
        Guid vocabularyExampleId,
        LanguageCode targetLanguageCode,
        string userAnswer,
        string correctAnswer,
        DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        if (vocabularyItemId == Guid.Empty)
        {
            throw new DomainException("Vocabulary item id is required.");
        }

        if (vocabularyExampleId == Guid.Empty)
        {
            throw new DomainException("Vocabulary example id is required.");
        }

        var normalizedUserAnswer = (userAnswer ?? string.Empty).Trim().ToLowerInvariant();
        var normalizedCorrectAnswer = (correctAnswer ?? string.Empty).Trim().ToLowerInvariant();
        var isCorrect = normalizedUserAnswer == normalizedCorrectAnswer;

        return new ExampleFillBlankAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VocabularyItemId = vocabularyItemId,
            VocabularyExampleId = vocabularyExampleId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            UserAnswer = normalizedUserAnswer,
            CorrectAnswer = normalizedCorrectAnswer,
            IsCorrect = isCorrect,
            Score = isCorrect ? 100 : 0,
            AttemptedAtUtc = utcNow
        };
    }
}
