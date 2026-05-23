using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

namespace EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Entities;

public sealed class ConversationLineTranslation : Entity<Guid>
{
    public Guid ConversationLineId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public string? ExpectedResponseHint { get; private set; }

    private ConversationLineTranslation() { }

    public static ConversationLineTranslation Create(Guid lineId, string languageCode, string text, string? expectedResponseHint, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            ConversationLineId = LessonTranslation.EnsureId(lineId, "Conversation line id"),
            LanguageCode = LessonTranslation.NormalizeLanguage(languageCode),
            Text = LessonTranslation.NormalizeRequired(text, 2000, "Line translation text"),
            ExpectedResponseHint = string.IsNullOrWhiteSpace(expectedResponseHint) ? null : expectedResponseHint.Trim(),
            CreatedAtUtc = utcNow
        };
}
