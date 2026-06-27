using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState.Repositories;
using EnglishTutor.Quota.Infrastructure.Persistence;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of the IUserQuotaStateRepository.
/// </summary>
public class UserQuotaStateRepository : IUserQuotaStateRepository
{
    private readonly QuotaDbContext _dbContext;

    public UserQuotaStateRepository(QuotaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserQuotaState?> GetByUserIdAndQuotaDateAsync(Guid userId, DateTime quotaDate, CancellationToken ct = default)
    {
        return await _dbContext.UserQuotaStates
            .FirstOrDefaultAsync(s => s.UserId == userId && s.QuotaDate == quotaDate, ct);
    }

    public async Task<UserQuotaState?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.UserQuotaStates
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task AddAsync(UserQuotaState state, CancellationToken ct = default)
    {
        await _dbContext.UserQuotaStates.AddAsync(state, ct);
    }

    public void Update(UserQuotaState state)
    {
        _dbContext.UserQuotaStates.Update(state);
    }
}