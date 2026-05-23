using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Abstractions;

public interface IUserVocabularyMasteryRepository
{
    Task<UserVocabularyMastery?> GetAsync(Guid userId, Guid vocabularyItemId, string targetLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserVocabularyMastery>> GetByUserAndTargetLanguageAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserVocabularyMastery>> GetDueAsync(Guid userId, string targetLanguageCode, DateTime nowUtc, CancellationToken cancellationToken);

    Task AddAsync(UserVocabularyMastery mastery, CancellationToken cancellationToken);

    Task<int> GetTodayAssignedCountAsync(Guid userId, string targetLanguageCode, DateTime todayStartUtc, CancellationToken cancellationToken);
}
