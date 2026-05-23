using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;

public sealed class ExampleFillBlankAttemptRepository(VocabularyDbContext dbContext) : IExampleFillBlankAttemptRepository
{
    public Task AddAsync(ExampleFillBlankAttempt attempt, CancellationToken cancellationToken)
    {
        dbContext.ExampleFillBlankAttempts.Add(attempt);
        return Task.CompletedTask;
    }
}
