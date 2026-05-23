using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

public sealed class LessonSectionTranslation : Entity<Guid>
{
    public Guid LessonSectionId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;

    private LessonSectionTranslation() { }

    public static LessonSectionTranslation Create(Guid sectionId, string languageCode, string title, string content, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            LessonSectionId = LessonTranslation.EnsureId(sectionId, "Lesson section id"),
            LanguageCode = LessonTranslation.NormalizeLanguage(languageCode),
            Title = LessonTranslation.NormalizeRequired(title, 200, "Section translation title"),
            Content = LessonTranslation.NormalizeRequired(content, 8000, "Section translation content"),
            CreatedAtUtc = utcNow
        };
}
