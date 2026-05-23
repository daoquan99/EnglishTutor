namespace EnglishTutor.Modules.Speaking.Domain.Events;

public sealed record SpeakingCorrectionMistake(
    string Type,
    string Original,
    string Corrected,
    string Explanation);
