using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;

namespace EnglishTutor.Modules.LearningContent.Domain.Quiz;

public sealed class Quiz : AggregateRoot<Guid>
{
    public Guid LessonId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string QuestionsJson { get; private set; } = string.Empty;

    private Quiz() { }

    public static Quiz Create(Guid lessonId, string title, string questionsJson, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            LessonId = LessonTranslation.EnsureId(lessonId, "Lesson id"),
            Title = LessonTranslation.NormalizeRequired(title, 200, "Quiz title"),
            QuestionsJson = LessonTranslation.NormalizeRequired(questionsJson, 8000, "Quiz questions JSON"),
            CreatedAtUtc = utcNow
        };
}
