using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Abstractions;

public interface IVocabularyReviewRepository
{
    Task AddAttemptAsync(VocabularyReviewAttempt attempt, CancellationToken cancellationToken);
}
