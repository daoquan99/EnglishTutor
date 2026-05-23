using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;

public sealed class VocabularyReviewRepository(VocabularyDbContext dbContext) : IVocabularyReviewRepository
{
    public Task AddAttemptAsync(VocabularyReviewAttempt attempt, CancellationToken cancellationToken)
    {
        dbContext.VocabularyReviewAttempts.Add(attempt);
        return Task.CompletedTask;
    }
}
