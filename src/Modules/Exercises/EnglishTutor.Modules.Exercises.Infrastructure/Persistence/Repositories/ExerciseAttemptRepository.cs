using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Exercises.Infrastructure.Persistence.Repositories;

public sealed class ExerciseAttemptRepository(ExercisesDbContext dbContext) : IExerciseAttemptRepository
{
    public Task<UserExerciseAttempt?> GetByIdWithAnswersAsync(Guid attemptId, CancellationToken cancellationToken) =>
        dbContext.UserExerciseAttempts
            .Include(attempt => attempt.Answers)
            .SingleOrDefaultAsync(attempt => attempt.Id == attemptId, cancellationToken);

    public Task<UserExerciseResult?> GetResultByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken) =>
        dbContext.UserExerciseResults.SingleOrDefaultAsync(result => result.AttemptId == attemptId, cancellationToken);

    public async Task AddAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken) =>
        await dbContext.UserExerciseAttempts.AddAsync(attempt, cancellationToken);

    public async Task AddResultAsync(UserExerciseResult result, CancellationToken cancellationToken) =>
        await dbContext.UserExerciseResults.AddAsync(result, cancellationToken);
}
