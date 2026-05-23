using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;

public sealed class NotificationTemplate : AggregateRoot<Guid>
{
    public NotificationType Type { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string TitleTemplate { get; private set; } = string.Empty;
    public string BodyTemplate { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private NotificationTemplate() { }

    public static NotificationTemplate Create(
        NotificationType type,
        string languageCode,
        string titleTemplate,
        string bodyTemplate,
        DateTime utcNow)
    {
        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Type = type,
            LanguageCode = NormalizeLanguage(languageCode),
            TitleTemplate = NormalizeRequired(titleTemplate, 300, "Title template"),
            BodyTemplate = NormalizeRequired(bodyTemplate, 2000, "Body template"),
            IsActive = true,
            CreatedAtUtc = utcNow
        };
    }

    public void Update(string titleTemplate, string bodyTemplate, bool isActive, DateTime utcNow)
    {
        TitleTemplate = NormalizeRequired(titleTemplate, 300, "Title template");
        BodyTemplate = NormalizeRequired(bodyTemplate, 2000, "Body template");
        IsActive = isActive;
        UpdatedAtUtc = utcNow;
    }

    private static string NormalizeLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode) || languageCode.Trim().Length is < 2 or > 3)
        {
            throw new DomainException("Language code must be 2-3 characters.");
        }

        return languageCode.Trim().ToLowerInvariant();
    }

    private static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
