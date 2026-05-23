namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record MistakeDetail(
    string Type,
    string Original,
    string Corrected,
    string Explanation);
