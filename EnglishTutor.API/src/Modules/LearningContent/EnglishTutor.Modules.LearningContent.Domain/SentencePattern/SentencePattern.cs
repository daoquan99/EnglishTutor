using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

namespace EnglishTutor.Modules.LearningContent.Domain.SentencePattern;

public sealed class SentencePattern : AggregateRoot<Guid>
{
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public LanguageLevel Level { get; private set; }
    public string Pattern { get; private set; } = string.Empty;
    public string Explanation { get; private set; } = string.Empty;
    public string Examples { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;

    private SentencePattern() { }

    public static SentencePattern Create(string targetLanguageCode, LanguageLevel level, string pattern, string explanation, string examples, string topic, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            TargetLanguageCode = LessonTranslation.NormalizeLanguage(targetLanguageCode),
            Level = level,
            Pattern = LessonTranslation.NormalizeRequired(pattern, 500, "Sentence pattern"),
            Explanation = LessonTranslation.NormalizeRequired(explanation, 2000, "Sentence pattern explanation"),
            Examples = LessonTranslation.NormalizeRequired(examples, 4000, "Sentence pattern examples"),
            Topic = LessonTranslation.NormalizeRequired(topic, 100, "Topic"),
            CreatedAtUtc = utcNow
        };
}
