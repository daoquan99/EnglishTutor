using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;

public sealed class PronunciationAttemptRepository(VocabularyDbContext dbContext) : IPronunciationAttemptRepository
{
    public Task AddVocabularyAttemptAsync(VocabularyPronunciationAttempt attempt, CancellationToken cancellationToken)
    {
        dbContext.VocabularyPronunciationAttempts.Add(attempt);
        return Task.CompletedTask;
    }

    public Task AddExampleAttemptAsync(ExampleSentencePronunciationAttempt attempt, CancellationToken cancellationToken)
    {
        dbContext.ExampleSentencePronunciationAttempts.Add(attempt);
        return Task.CompletedTask;
    }
}
