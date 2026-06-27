using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent.Repositories;

namespace EnglishTutor.Quota.Application.Abstractions.Persistence;

/// <summary>
/// Unit of work for the Quota module.
/// </summary>
public interface IQuotaUnitOfWork : IDisposable
{
    IUserQuotaRuleRepository UserQuotaRules { get; }
    IUserQuotaStateRepository UserQuotaStates { get; }
    IQuotaReservationRepository QuotaReservations { get; }
    IUsageLogRepository UsageLogs { get; }
    IRateLimitEventRepository RateLimitEvents { get; }

    Task<int> SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}