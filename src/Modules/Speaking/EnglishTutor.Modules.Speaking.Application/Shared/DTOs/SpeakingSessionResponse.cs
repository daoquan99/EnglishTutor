namespace EnglishTutor.Modules.Speaking.Application.Shared.DTOs;

public sealed record SpeakingSessionResponse(
    Guid SessionId,
    string SessionType,
    string? Topic,
    string TargetLanguageCode,
    string UserLevel,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    Guid? ConversationScenarioId,
    int? CurrentLineOrder);
