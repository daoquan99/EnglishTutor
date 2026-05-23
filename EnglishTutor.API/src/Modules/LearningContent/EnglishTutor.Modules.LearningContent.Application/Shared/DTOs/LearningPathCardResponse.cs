namespace EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;

public sealed record LearningPathCardResponse(
    Guid Id,
    string TargetLanguageCode,
    string ContentType,
    Guid ContentId,
    string Title,
    string Level,
    string? Skill,
    string Status,
    int Order,
    DateTime? CompletedAtUtc,
    DateTime LastUpdatedAtUtc);
