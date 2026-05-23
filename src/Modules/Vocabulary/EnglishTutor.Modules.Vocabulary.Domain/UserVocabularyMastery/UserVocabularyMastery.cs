using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Domain.Enums;
using EnglishTutor.Modules.Vocabulary.Domain.Events;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class UserVocabularyMastery : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid VocabularyItemId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public int MeaningMasteryScore { get; private set; }
    public int PronunciationMasteryScore { get; private set; }
    public int ExampleSentenceScore { get; private set; }
    public int ReviewCount { get; private set; }
    public int CorrectReviewCount { get; private set; }
    public DateTime? LastReviewedAtUtc { get; private set; }
    public DateTime NextReviewAtUtc { get; private set; }
    public VocabularyMasteryStatus Status { get; private set; }

    private UserVocabularyMastery() { }

    public static UserVocabularyMastery Create(Guid userId, Guid vocabularyItemId, LanguageCode targetLanguageCode, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        if (vocabularyItemId == Guid.Empty)
        {
            throw new DomainException("Vocabulary item id is required.");
        }

        return new UserVocabularyMastery
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VocabularyItemId = vocabularyItemId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Status = VocabularyMasteryStatus.New,
            NextReviewAtUtc = utcNow
        };
    }

    public void RecordReview(bool isCorrect, int score, DateTime utcNow)
    {
        var normalizedScore = Math.Clamp(score, 0, 100);

        ReviewCount++;
        if (isCorrect)
        {
            CorrectReviewCount++;
        }

        MeaningMasteryScore = CalculateMovingScore(MeaningMasteryScore, normalizedScore);
        LastReviewedAtUtc = utcNow;
        Status = ResolveStatus(isCorrect, normalizedScore);
        NextReviewAtUtc = utcNow.Add(CalculateReviewInterval());

        AddDomainEvent(new VocabularyReviewedDomainEvent(
            UserId,
            VocabularyItemId,
            TargetLanguageCode.Value,
            isCorrect,
            normalizedScore,
            Status.ToString(),
            utcNow));

        if (Status == VocabularyMasteryStatus.Mastered)
        {
            AddDomainEvent(new VocabularyMasteredDomainEvent(UserId, VocabularyItemId, TargetLanguageCode.Value, utcNow));
        }
    }

    public void RecordPronunciationScore(int pronunciationScore, int exampleSentenceScore)
    {
        PronunciationMasteryScore = Math.Clamp(pronunciationScore, 0, 100);
        ExampleSentenceScore = Math.Clamp(exampleSentenceScore, 0, 100);
    }

    public void MarkMastered(DateTime utcNow)
    {
        if (Status == VocabularyMasteryStatus.Mastered)
        {
            return;
        }

        Status = VocabularyMasteryStatus.Mastered;
        MeaningMasteryScore = Math.Max(MeaningMasteryScore, 90);
        NextReviewAtUtc = utcNow.AddDays(30);

        AddDomainEvent(new VocabularyMasteredDomainEvent(UserId, VocabularyItemId, TargetLanguageCode.Value, utcNow));
    }

    private VocabularyMasteryStatus ResolveStatus(bool isCorrect, int score)
    {
        if (!isCorrect || score < 50)
        {
            return VocabularyMasteryStatus.Weak;
        }

        if (ReviewCount >= 5 && CorrectReviewCount >= 5 && MeaningMasteryScore >= 85)
        {
            return VocabularyMasteryStatus.Mastered;
        }

        return ReviewCount >= 3 ? VocabularyMasteryStatus.Reviewing : VocabularyMasteryStatus.Learning;
    }

    private TimeSpan CalculateReviewInterval() =>
        Status switch
        {
            VocabularyMasteryStatus.Weak => TimeSpan.FromHours(6),
            VocabularyMasteryStatus.Learning => TimeSpan.FromDays(1),
            VocabularyMasteryStatus.Reviewing => TimeSpan.FromDays(Math.Min(14, ReviewCount * 2)),
            VocabularyMasteryStatus.Mastered => TimeSpan.FromDays(30),
            _ => TimeSpan.FromDays(1)
        };

    private static int CalculateMovingScore(int currentScore, int newScore) =>
        currentScore == 0 ? newScore : (int)Math.Round((currentScore * 0.7m) + (newScore * 0.3m));
}
