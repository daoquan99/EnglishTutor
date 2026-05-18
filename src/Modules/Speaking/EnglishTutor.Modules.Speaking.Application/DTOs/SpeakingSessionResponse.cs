namespace EnglishTutor.Modules.Speaking.Application.DTOs;

public sealed record SpeakingSessionResponse(
    Guid SessionId,
    string SessionType,
    string? Topic,
    string TargetLanguageCode,
    string UserLevel,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);
