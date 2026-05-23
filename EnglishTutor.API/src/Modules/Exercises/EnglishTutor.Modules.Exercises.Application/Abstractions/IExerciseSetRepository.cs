using EnglishTutor.Modules.Exercises.Domain.ExerciseSet;

namespace EnglishTutor.Modules.Exercises.Application.Abstractions;

public interface IExerciseSetRepository
{
    Task<ExerciseSet?> GetByIdWithQuestionsAsync(Guid exerciseSetId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExerciseSet>> ListPublishedAsync(
        int page,
        int pageSize,
        string? level,
        string? type,
        string? topic,
        string? skill,
        string? targetLanguageCode,
        CancellationToken cancellationToken);

    Task AddAsync(ExerciseSet exerciseSet, CancellationToken cancellationToken);
}
