using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;

namespace EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule.Repositories;

/// <summary>
/// Repository interface for UserQuotaRule aggregate.
/// </summary>
public interface IUserQuotaRuleRepository
{
    Task<UserQuotaRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserQuotaRule?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<UserQuotaRule>> GetAllActiveAsync(CancellationToken ct = default);
    Task AddAsync(UserQuotaRule rule, CancellationToken ct = default);
    void Update(UserQuotaRule rule);
    void Delete(UserQuotaRule rule);
}