using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Speaking.Domain.Entities;

public sealed class SpeakingTurnResult : Entity<Guid>
{
    public Guid SpeakingTurnId { get; private set; }
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public string OriginalText { get; private set; } = string.Empty;
    public string CorrectedText { get; private set; } = string.Empty;
    public string NaturalVersion { get; private set; } = string.Empty;
    public int GrammarScore { get; private set; }
    public int VocabularyScore { get; private set; }
    public int PronunciationScore { get; private set; }
    public int FluencyScore { get; private set; }
    public int TaskCompletionScore { get; private set; }
    public int OverallScore { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
    public string FeedbackLanguageCode { get; private set; } = string.Empty;
    public string? AudioUrl { get; private set; }
    public string? RecognizedText { get; private set; }
    public string? WordLevelFeedbackJson { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private SpeakingTurnResult() { }

    public static SpeakingTurnResult Create(
        Guid speakingTurnId,
        Guid userId,
        LanguageCode targetLanguageCode,
        string originalText,
        string correctedText,
        string naturalVersion,
        int grammarScore,
        int vocabularyScore,
        int pronunciationScore,
        int fluencyScore,
        int taskCompletionScore,
        string feedback,
        string feedbackLanguageCode,
        string? audioUrl,
        string? recognizedText,
        string? wordLevelFeedbackJson)
    {
        if (speakingTurnId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Speaking turn id and user id are required.");
        }

        var normalizedGrammarScore = Math.Clamp(grammarScore, 0, 100);
        var normalizedVocabularyScore = Math.Clamp(vocabularyScore, 0, 100);
        var normalizedPronunciationScore = Math.Clamp(pronunciationScore, 0, 100);
        var normalizedFluencyScore = Math.Clamp(fluencyScore, 0, 100);
        var normalizedTaskCompletionScore = Math.Clamp(taskCompletionScore, 0, 100);

        return new SpeakingTurnResult
        {
            Id = Guid.NewGuid(),
            SpeakingTurnId = speakingTurnId,
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            OriginalText = SpeakingTurn.NormalizeRequired(originalText, 4000, "Original text"),
            CorrectedText = SpeakingTurn.NormalizeRequired(correctedText, 4000, "Corrected text"),
            NaturalVersion = SpeakingTurn.NormalizeRequired(naturalVersion, 4000, "Natural version"),
            GrammarScore = normalizedGrammarScore,
            VocabularyScore = normalizedVocabularyScore,
            PronunciationScore = normalizedPronunciationScore,
            FluencyScore = normalizedFluencyScore,
            TaskCompletionScore = normalizedTaskCompletionScore,
            OverallScore = CalculateOverallScore(
                normalizedGrammarScore,
                normalizedVocabularyScore,
                normalizedPronunciationScore,
                normalizedFluencyScore,
                normalizedTaskCompletionScore),
            Feedback = SpeakingTurn.NormalizeRequired(feedback, 4000, "Feedback"),
            FeedbackLanguageCode = SpeakingTurn.NormalizeRequired(feedbackLanguageCode, 3, "Feedback language code"),
            AudioUrl = SpeakingTurn.NormalizeOptional(audioUrl, 2048, "Audio url"),
            RecognizedText = SpeakingTurn.NormalizeOptional(recognizedText, 4000, "Recognized text"),
            WordLevelFeedbackJson = SpeakingTurn.NormalizeOptional(wordLevelFeedbackJson, 8000, "Word level feedback"),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static int CalculateOverallScore(params int[] scores) =>
        (int)Math.Round(scores.Average());
}
