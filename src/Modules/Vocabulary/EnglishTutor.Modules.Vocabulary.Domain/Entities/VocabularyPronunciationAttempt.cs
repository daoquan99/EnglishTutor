using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyPronunciationAttempt : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid VocabularyItemId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public string? AudioUrl { get; private set; }
    public string RecognizedText { get; private set; } = string.Empty;
    public int PronunciationScore { get; private set; }
    public int AccuracyScore { get; private set; }
    public int FluencyScore { get; private set; }
    public int CompletenessScore { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
    public DateTime AttemptedAtUtc { get; private set; }

    private VocabularyPronunciationAttempt() { }

    public static VocabularyPronunciationAttempt Create(
        Guid userId,
        Guid vocabularyItemId,
        LanguageCode targetLanguageCode,
        string? audioUrl,
        string recognizedText,
        int pronunciationScore,
        int accuracyScore,
        int fluencyScore,
        int completenessScore,
        string feedback)
    {
        if (userId == Guid.Empty || vocabularyItemId == Guid.Empty)
        {
            throw new DomainException("User id and vocabulary item id are required.");
        }

        var attempt = new VocabularyPronunciationAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VocabularyItemId = vocabularyItemId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            AudioUrl = VocabularyItem.NormalizeOptional(audioUrl, 2048, "Audio url"),
            RecognizedText = VocabularyItem.NormalizeRequired(recognizedText, 1000, "Recognized text"),
            PronunciationScore = Math.Clamp(pronunciationScore, 0, 100),
            AccuracyScore = Math.Clamp(accuracyScore, 0, 100),
            FluencyScore = Math.Clamp(fluencyScore, 0, 100),
            CompletenessScore = Math.Clamp(completenessScore, 0, 100),
            Feedback = VocabularyItem.NormalizeRequired(feedback, 2000, "Feedback"),
            AttemptedAtUtc = DateTime.UtcNow
        };

        attempt.AddDomainEvent(new Events.VocabularyPronunciationPracticedDomainEvent(
            userId,
            vocabularyItemId,
            attempt.TargetLanguageCode.Value,
            attempt.PronunciationScore,
            attempt.AccuracyScore,
            attempt.FluencyScore,
            attempt.AttemptedAtUtc));

        return attempt;
    }
}
