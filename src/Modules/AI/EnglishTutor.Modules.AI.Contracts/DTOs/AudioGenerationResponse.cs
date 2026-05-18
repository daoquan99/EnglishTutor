namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record AudioGenerationResponse(
    string AudioUrl,
    string ContentType,
    TimeSpan Duration);
