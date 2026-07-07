using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress;

public sealed class LearnerLanguageProgress : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid LanguagePairId { get; private set; }
    public string NativeLanguageCode { get; private set; } = string.Empty;
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public int SessionsCompleted { get; private set; }
    public long SpeakingSeconds { get; private set; }
    public int FeedbackCount { get; private set; }
    public int? LatestScore { get; private set; }
    public string? CurrentCefrLevel { get; private set; }
    public DateTime? LastPracticedAtUtc { get; private set; }
    public long Version { get; private set; }

    private LearnerLanguageProgress()
    {
    }

    public static LearnerLanguageProgress Create(
        Guid id,
        Guid userId,
        Guid languagePairId,
        string nativeLanguageCode,
        string targetLanguageCode)
    {
        return new LearnerLanguageProgress
        {
            Id = id,
            UserId = userId,
            LanguagePairId = languagePairId,
            NativeLanguageCode = nativeLanguageCode,
            TargetLanguageCode = targetLanguageCode,
            Version = 1
        };
    }

    public void RecordSession(int durationSeconds, DateTime occurredAtUtc)
    {
        SessionsCompleted++;
        SpeakingSeconds += Math.Max(0, durationSeconds);
        LastPracticedAtUtc = occurredAtUtc;
        Version++;
    }

    public void RecordFeedback(int? score, string? cefrLevel)
    {
        FeedbackCount++;
        LatestScore = score;
        CurrentCefrLevel = string.IsNullOrWhiteSpace(cefrLevel) ? CurrentCefrLevel : cefrLevel;
        Version++;
    }
}
