using EnglishTutor.Modules.LearningContent.Domain.Lesson;

namespace EnglishTutor.Modules.LearningContent.Application.Abstractions;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdWithDetailsAsync(Guid lessonId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Lesson>> ListPublishedAsync(
        int page,
        int pageSize,
        string? level,
        string? topic,
        string? skill,
        string? targetLanguageCode,
        CancellationToken cancellationToken);
}
