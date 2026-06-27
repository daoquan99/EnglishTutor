using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;

namespace EnglishTutor.Quota.Domain.Aggregates.UserQuotaState.Repositories;

/// <summary>
/// Repository interface for UserQuotaState.
/// </summary>
public interface IUserQuotaStateRepository
{
    Task<UserQuotaState?> GetByUserIdAndQuotaDateAsync(Guid userId, DateTime quotaDate, CancellationToken ct = default);
    Task<UserQuotaState?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(UserQuotaState state, CancellationToken ct = default);
    void Update(UserQuotaState state);
}