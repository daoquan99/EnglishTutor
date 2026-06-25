using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog.Repositories;

namespace EnglishTutor.Quota.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of the IUsageLogRepository.
/// </summary>
public class UsageLogRepository : IUsageLogRepository
{
    private readonly QuotaDbContext _dbContext;

    public UsageLogRepository(QuotaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(UsageLog usageLog, CancellationToken ct = default)
    {
        await _dbContext.UsageLogs.AddAsync(usageLog, ct);
    }
}
