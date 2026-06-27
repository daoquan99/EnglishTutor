using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Application.Abstractions.Persistence;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent.Repositories;
using EnglishTutor.Quota.Infrastructure.Persistence.Repositories;

namespace EnglishTutor.Quota.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core implementation of the IQuotaUnitOfWork.
/// </summary>
public class QuotaUnitOfWork : IQuotaUnitOfWork, IDisposable
{
    private readonly QuotaDbContext _context;
    private IUserQuotaRuleRepository? _userQuotaRules;
    private IUserQuotaStateRepository? _userQuotaStates;
    private IQuotaReservationRepository? _quotaReservations;
    private IUsageLogRepository? _usageLogs;
    private IRateLimitEventRepository? _rateLimitEvents;

    public QuotaUnitOfWork(QuotaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IUserQuotaRuleRepository UserQuotaRules => _userQuotaRules ??= new UserQuotaRuleRepository(_context);

    public IUserQuotaStateRepository UserQuotaStates => _userQuotaStates ??= new UserQuotaStateRepository(_context);

    public IQuotaReservationRepository QuotaReservations => _quotaReservations ??= new QuotaReservationRepository(_context);

    public IUsageLogRepository UsageLogs => _usageLogs ??= new UsageLogRepository(_context);

    public IRateLimitEventRepository RateLimitEvents => _rateLimitEvents ??= new RateLimitEventRepository(_context);

    public Task<int> SaveChanges()
    {
        return Task.FromResult(_context.SaveChanges());
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}