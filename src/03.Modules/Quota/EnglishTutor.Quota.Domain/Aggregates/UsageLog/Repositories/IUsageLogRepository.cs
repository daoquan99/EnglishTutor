using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;

namespace EnglishTutor.Quota.Domain.Aggregates.UsageLog.Repositories;

/// <summary>
/// Repository interface for UsageLog aggregate.
/// </summary>
public interface IUsageLogRepository
{
    Task AddAsync(UsageLog usageLog, CancellationToken ct = default);
}