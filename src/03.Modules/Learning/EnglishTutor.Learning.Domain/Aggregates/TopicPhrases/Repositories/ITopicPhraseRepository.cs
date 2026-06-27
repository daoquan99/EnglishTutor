using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;

public interface ITopicPhraseRepository
{
    Task<TopicPhrase?> GetByIdForTopicAsync(Guid topicId, Guid phraseId, bool includeDeleted, CancellationToken ct = default);
    Task<bool> ExistsByPhraseAsync(Guid topicId, string normalizedPhrase, Guid? excludePhraseId, CancellationToken ct = default);
    Task<IReadOnlyList<TopicPhrase>> ListByTopicIdAsync(Guid topicId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<TopicPhrase>> ListByTopicIdAdminAsync(Guid topicId, int page, int pageSize, bool includeInactive, bool includeDeleted, CancellationToken ct = default);
    Task<int> CountByTopicIdAsync(Guid topicId, bool activeOnly, bool includeDeleted, CancellationToken ct = default);
    void Add(TopicPhrase phrase);
}
