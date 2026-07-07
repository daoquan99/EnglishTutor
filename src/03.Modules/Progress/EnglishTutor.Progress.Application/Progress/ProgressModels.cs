namespace EnglishTutor.Progress.Application.Progress;

public sealed record LearnerLanguageProgressView(
    Guid LanguagePairId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    int SessionsCompleted,
    long SpeakingSeconds,
    int FeedbackCount,
    int? LatestScore,
    string? CurrentCefrLevel,
    DateTime? LastPracticedAtUtc);
