using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Events;

namespace EnglishTutor.Modules.LearningContent.Domain.Lesson;

public sealed class Lesson : AggregateRoot<Guid>
{
    private readonly List<LessonTranslation> _translations = [];
    private readonly List<LessonSection> _sections = [];

    public string TargetLanguageCode { get; private set; } = string.Empty;
    public LanguageLevel Level { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public LearningSkill Skill { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public bool IsPublished { get; private set; }
    public IReadOnlyCollection<LessonTranslation> Translations => _translations.AsReadOnly();
    public IReadOnlyCollection<LessonSection> Sections => _sections.AsReadOnly();

    private Lesson() { }

    public static Lesson Create(
        string targetLanguageCode,
        LanguageLevel level,
        string topic,
        LearningSkill skill,
        string title,
        string description,
        int order,
        int estimatedMinutes,
        DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            TargetLanguageCode = LessonTranslation.NormalizeLanguage(targetLanguageCode),
            Level = level,
            Topic = LessonTranslation.NormalizeRequired(topic, 100, "Topic"),
            Skill = skill,
            Title = LessonTranslation.NormalizeRequired(title, 200, "Lesson title"),
            Description = LessonTranslation.NormalizeRequired(description, 1000, "Lesson description"),
            Order = order,
            EstimatedMinutes = Math.Clamp(estimatedMinutes, 1, 240),
            CreatedAtUtc = utcNow
        };

    public LessonSection AddSection(string title, string content, int order, string sectionType, DateTime utcNow)
    {
        var section = LessonSection.Create(Id, title, content, order, sectionType, utcNow);
        _sections.Add(section);
        return section;
    }

    public void AddTranslation(string languageCode, string title, string description, DateTime utcNow)
    {
        if (_translations.Any(translation => translation.LanguageCode == LessonTranslation.NormalizeLanguage(languageCode)))
        {
            return;
        }

        _translations.Add(LessonTranslation.Create(Id, languageCode, title, description, utcNow));
    }

    public void Publish(DateTime utcNow)
    {
        if (_sections.Count == 0)
        {
            throw new DomainException("Lesson must have at least one section before publishing.");
        }

        if (IsPublished)
        {
            return;
        }

        IsPublished = true;
        UpdatedAtUtc = utcNow;
        AddDomainEvent(new LessonPublishedDomainEvent(Id, TargetLanguageCode, Level.ToString(), Topic, Skill.ToString(), utcNow));
    }
}
