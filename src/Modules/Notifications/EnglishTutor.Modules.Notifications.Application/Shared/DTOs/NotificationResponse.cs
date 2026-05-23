namespace EnglishTutor.Modules.Notifications.Application.Shared.DTOs;

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? Data,
    bool IsRead,
    string Channel,
    string Status,
    DateTime ScheduledAtUtc,
    DateTime? SentAtUtc,
    DateTime? ReadAtUtc);
