using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Topic?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default);
    Task<Topic?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Topic?> GetBySlugAsync(string slug, bool includeDeleted, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> ExistsActiveTopicAsync(Guid topicId, CancellationToken ct = default);
    Task<IReadOnlyList<Topic>> ListActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Topic>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default);
    Task AddAsync(Topic topic, CancellationToken ct = default);
    void AddTopicMode(TopicMode topicMode);
}
