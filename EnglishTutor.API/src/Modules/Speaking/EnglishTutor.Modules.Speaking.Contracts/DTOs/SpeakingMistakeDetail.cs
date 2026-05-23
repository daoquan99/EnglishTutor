namespace EnglishTutor.Modules.Speaking.Contracts.DTOs;

public sealed record SpeakingMistakeDetail(
    string Type,
    string Original,
    string Corrected,
    string Explanation);
