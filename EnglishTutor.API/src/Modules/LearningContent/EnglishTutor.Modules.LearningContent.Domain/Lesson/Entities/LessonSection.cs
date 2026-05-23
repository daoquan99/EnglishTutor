using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

public sealed class LessonSection : Entity<Guid>
{
    private readonly List<LessonSectionTranslation> _translations = [];

    public Guid LessonId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public string SectionType { get; private set; } = string.Empty;
    public IReadOnlyCollection<LessonSectionTranslation> Translations => _translations.AsReadOnly();

    private LessonSection() { }

    public static LessonSection Create(Guid lessonId, string title, string content, int order, string sectionType, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            LessonId = LessonTranslation.EnsureId(lessonId, "Lesson id"),
            Title = LessonTranslation.NormalizeRequired(title, 200, "Section title"),
            Content = LessonTranslation.NormalizeRequired(content, 8000, "Section content"),
            Order = order,
            SectionType = LessonTranslation.NormalizeRequired(sectionType, 50, "Section type"),
            CreatedAtUtc = utcNow
        };

    public void AddTranslation(string languageCode, string title, string content, DateTime utcNow)
    {
        if (_translations.Any(translation => translation.LanguageCode == LessonTranslation.NormalizeLanguage(languageCode)))
        {
            return;
        }

        _translations.Add(LessonSectionTranslation.Create(Id, languageCode, title, content, utcNow));
    }
}
