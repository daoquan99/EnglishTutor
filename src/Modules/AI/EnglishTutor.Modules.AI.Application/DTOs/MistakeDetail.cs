namespace EnglishTutor.Modules.AI.Application.DTOs;

public sealed record MistakeDetail(
    string Type,
    string Original,
    string Corrected,
    string Explanation);
