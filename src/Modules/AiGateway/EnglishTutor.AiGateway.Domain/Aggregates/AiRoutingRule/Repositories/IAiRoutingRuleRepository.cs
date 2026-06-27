using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule.Repositories;

/// <summary>
/// Repository interface for AiRoutingRule aggregate.
/// </summary>
public interface IAiRoutingRuleRepository
{
    Task<AiRoutingRule?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Finds the active routing rule matching the activity/topic/scenario context
    /// (codes are normalised lower-case). Returns null when no active rule matches.
    /// </summary>
    Task<AiRoutingRule?> FindActiveMatchAsync(
        string activityType,
        string topicCode,
        string scenarioCode,
        CancellationToken ct = default);

    Task<IReadOnlyList<AiRoutingRule>> ListAsync(CancellationToken ct = default);

    Task AddAsync(AiRoutingRule rule, CancellationToken ct = default);
    void Update(AiRoutingRule rule);
}
