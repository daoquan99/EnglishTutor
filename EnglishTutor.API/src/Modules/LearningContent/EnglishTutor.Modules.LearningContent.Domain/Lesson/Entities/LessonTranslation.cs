using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

public sealed class LessonTranslation : Entity<Guid>
{
    public Guid LessonId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private LessonTranslation() { }

    public static LessonTranslation Create(Guid lessonId, string languageCode, string title, string description, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            LessonId = EnsureId(lessonId, "Lesson id"),
            LanguageCode = NormalizeLanguage(languageCode),
            Title = NormalizeRequired(title, 200, "Lesson translation title"),
            Description = NormalizeRequired(description, 1000, "Lesson translation description"),
            CreatedAtUtc = utcNow
        };

    internal static Guid EnsureId(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{fieldName} is required.");
        }

        return value;
    }

    internal static string NormalizeLanguage(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length is < 2 or > 3)
        {
            throw new DomainException("Language code must be 2-3 characters.");
        }

        return value.Trim().ToLowerInvariant();
    }

    internal static string NormalizeRequired(string value, int maxLength, string fieldName)
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
