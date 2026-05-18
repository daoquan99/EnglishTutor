using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class ExampleSentencePronunciationAttempt : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid VocabularyExampleId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public string RecognizedText { get; private set; } = string.Empty;
    public int PronunciationScore { get; private set; }
    public int AccuracyScore { get; private set; }
    public int FluencyScore { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
    public DateTime AttemptedAtUtc { get; private set; }

    private ExampleSentencePronunciationAttempt() { }

    public static ExampleSentencePronunciationAttempt Create(
        Guid userId,
        Guid vocabularyExampleId,
        LanguageCode targetLanguageCode,
        string recognizedText,
        int pronunciationScore,
        int accuracyScore,
        int fluencyScore,
        string feedback)
    {
        if (userId == Guid.Empty || vocabularyExampleId == Guid.Empty)
        {
            throw new DomainException("User id and vocabulary example id are required.");
        }

        var attempt = new ExampleSentencePronunciationAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VocabularyExampleId = vocabularyExampleId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            RecognizedText = VocabularyItem.NormalizeRequired(recognizedText, 1000, "Recognized text"),
            PronunciationScore = Math.Clamp(pronunciationScore, 0, 100),
            AccuracyScore = Math.Clamp(accuracyScore, 0, 100),
            FluencyScore = Math.Clamp(fluencyScore, 0, 100),
            Feedback = VocabularyItem.NormalizeRequired(feedback, 2000, "Feedback"),
            AttemptedAtUtc = DateTime.UtcNow
        };

        attempt.AddDomainEvent(new Events.ExampleSentencePronunciationPracticedDomainEvent(
            userId,
            vocabularyExampleId,
            attempt.TargetLanguageCode.Value,
            attempt.PronunciationScore,
            attempt.AccuracyScore,
            attempt.FluencyScore,
            attempt.AttemptedAtUtc));

        return attempt;
    }
}
