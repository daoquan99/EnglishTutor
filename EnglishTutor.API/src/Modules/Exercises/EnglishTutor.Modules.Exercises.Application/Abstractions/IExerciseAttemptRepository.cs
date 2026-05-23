using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;

namespace EnglishTutor.Modules.Exercises.Application.Abstractions;

public interface IExerciseAttemptRepository
{
    Task<UserExerciseAttempt?> GetByIdWithAnswersAsync(Guid attemptId, CancellationToken cancellationToken);

    Task<UserExerciseResult?> GetResultByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken);

    Task AddAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken);

    Task AddResultAsync(UserExerciseResult result, CancellationToken cancellationToken);
}
