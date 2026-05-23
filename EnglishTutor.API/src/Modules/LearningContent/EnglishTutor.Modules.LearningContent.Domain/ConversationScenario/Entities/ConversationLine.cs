using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Enums;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

namespace EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Entities;

public sealed class ConversationLine : Entity<Guid>
{
    private readonly List<ConversationLineTranslation> _translations = [];

    public Guid ScenarioId { get; private set; }
    public ConversationSpeaker Speaker { get; private set; }
    public int Order { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public string? ExpectedResponseHint { get; private set; }
    public string? AudioUrl { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<ConversationLineTranslation> Translations => _translations.AsReadOnly();

    private ConversationLine() { }

    public static ConversationLine Create(
        Guid scenarioId,
        ConversationSpeaker speaker,
        int order,
        string text,
        string? expectedResponseHint,
        string? audioUrl,
        string? notes,
        DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            ScenarioId = LessonTranslation.EnsureId(scenarioId, "Scenario id"),
            Speaker = speaker,
            Order = order,
            Text = LessonTranslation.NormalizeRequired(text, 2000, "Conversation line text"),
            ExpectedResponseHint = string.IsNullOrWhiteSpace(expectedResponseHint) ? null : expectedResponseHint.Trim(),
            AudioUrl = string.IsNullOrWhiteSpace(audioUrl) ? null : audioUrl.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedAtUtc = utcNow
        };

    public void AddTranslation(string languageCode, string text, string? expectedResponseHint, DateTime utcNow)
    {
        if (_translations.Any(translation => translation.LanguageCode == LessonTranslation.NormalizeLanguage(languageCode)))
        {
            return;
        }

        _translations.Add(ConversationLineTranslation.Create(Id, languageCode, text, expectedResponseHint, utcNow));
    }
}
