using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule.Repositories;
using EnglishTutor.Quota.Infrastructure.Persistence;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of the IUserQuotaRuleRepository.
/// </summary>
public class UserQuotaRuleRepository : IUserQuotaRuleRepository
{
    private readonly QuotaDbContext _dbContext;

    public UserQuotaRuleRepository(QuotaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserQuotaRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.UserQuotaRules
            .IgnoreQueryFilters() // To include soft-deleted if needed, but we usually don't want them.
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<UserQuotaRule?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserQuotaRules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive, ct);
    }

    public async Task<IEnumerable<UserQuotaRule>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _dbContext.UserQuotaRules
            .Where(r => r.IsActive)
            .ToListAsync(ct);
    }

    public async Task AddAsync(UserQuotaRule rule, CancellationToken ct = default)
    {
        await _dbContext.UserQuotaRules.AddAsync(rule, ct);
        // Note: SaveChanges is called by the UnitOfWork.
    }

    public void Update(UserQuotaRule rule)
    {
        _dbContext.UserQuotaRules.Update(rule);
    }

    public void Delete(UserQuotaRule rule)
    {
        // Soft delete: mark as deleted.
        rule.MarkDeleted(null);
        _dbContext.UserQuotaRules.Update(rule);
    }
}