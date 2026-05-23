using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

namespace EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Entities;

public sealed class ConversationScenarioTranslation : Entity<Guid>
{
    public Guid ScenarioId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Setting { get; private set; } = string.Empty;

    private ConversationScenarioTranslation() { }

    public static ConversationScenarioTranslation Create(Guid scenarioId, string languageCode, string title, string description, string setting, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            ScenarioId = LessonTranslation.EnsureId(scenarioId, "Scenario id"),
            LanguageCode = LessonTranslation.NormalizeLanguage(languageCode),
            Title = LessonTranslation.NormalizeRequired(title, 200, "Scenario translation title"),
            Description = LessonTranslation.NormalizeRequired(description, 1000, "Scenario translation description"),
            Setting = LessonTranslation.NormalizeRequired(setting, 200, "Scenario translation setting"),
            CreatedAtUtc = utcNow
        };
}
