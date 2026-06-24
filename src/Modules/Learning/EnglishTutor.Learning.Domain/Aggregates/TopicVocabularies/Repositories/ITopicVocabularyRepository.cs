using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;

public interface ITopicVocabularyRepository
{
    Task<TopicVocabulary?> GetByIdForTopicAsync(Guid topicId, Guid vocabularyId, bool includeDeleted, CancellationToken ct = default);
    Task<bool> ExistsByWordAsync(Guid topicId, string normalizedWord, Guid? excludeVocabularyId, CancellationToken ct = default);
    Task<IReadOnlyList<TopicVocabulary>> ListByTopicIdAsync(Guid topicId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<TopicVocabulary>> ListByTopicIdAdminAsync(Guid topicId, int page, int pageSize, bool includeInactive, bool includeDeleted, CancellationToken ct = default);
    Task<int> CountByTopicIdAsync(Guid topicId, bool activeOnly, bool includeDeleted, CancellationToken ct = default);
    void Add(TopicVocabulary vocabulary);
}
