using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Entities;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Enums;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

namespace EnglishTutor.Modules.LearningContent.Domain.ConversationScenario;

public sealed class ConversationScenario : AggregateRoot<Guid>
{
    private readonly List<ConversationLine> _lines = [];
    private readonly List<ConversationScenarioTranslation> _translations = [];

    public string TargetLanguageCode { get; private set; } = string.Empty;
    public LanguageLevel Level { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Setting { get; private set; } = string.Empty;
    public int Difficulty { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public bool IsPublished { get; private set; }
    public IReadOnlyCollection<ConversationLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<ConversationScenarioTranslation> Translations => _translations.AsReadOnly();

    private ConversationScenario() { }

    public static ConversationScenario Create(
        string targetLanguageCode,
        LanguageLevel level,
        string title,
        string description,
        string setting,
        int difficulty,
        int estimatedMinutes,
        DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            TargetLanguageCode = LessonTranslation.NormalizeLanguage(targetLanguageCode),
            Level = level,
            Title = LessonTranslation.NormalizeRequired(title, 200, "Scenario title"),
            Description = LessonTranslation.NormalizeRequired(description, 1000, "Scenario description"),
            Setting = LessonTranslation.NormalizeRequired(setting, 200, "Scenario setting"),
            Difficulty = Math.Clamp(difficulty, 1, 5),
            EstimatedMinutes = Math.Clamp(estimatedMinutes, 1, 240),
            CreatedAtUtc = utcNow
        };

    public ConversationLine AddLine(ConversationSpeaker speaker, int order, string text, string? hint, string? audioUrl, string? notes, DateTime utcNow)
    {
        var line = ConversationLine.Create(Id, speaker, order, text, hint, audioUrl, notes, utcNow);
        _lines.Add(line);
        return line;
    }

    public void AddTranslation(string languageCode, string title, string description, string setting, DateTime utcNow)
    {
        if (_translations.Any(translation => translation.LanguageCode == LessonTranslation.NormalizeLanguage(languageCode)))
        {
            return;
        }

        _translations.Add(ConversationScenarioTranslation.Create(Id, languageCode, title, description, setting, utcNow));
    }

    public void Publish(DateTime utcNow)
    {
        if (_lines.Count == 0)
        {
            throw new DomainException("Conversation scenario must have at least one line before publishing.");
        }

        IsPublished = true;
        UpdatedAtUtc = utcNow;
    }
}
