using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule.Repositories;

/// <summary>
/// Repository interface for AiRoutingRule aggregate.
/// </summary>
public interface IAiRoutingRuleRepository
{
    Task<AiRoutingRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AiRoutingRule rule, CancellationToken ct = default);
    void Update(AiRoutingRule rule);
}
