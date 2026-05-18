using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence.Repositories;

public sealed class AiRequestLogRepository(AiDbContext dbContext) : IAiRequestLogRepository
{
    public async Task AddAsync(AiRequestLog requestLog, CancellationToken cancellationToken)
    {
        dbContext.AiRequestLogs.Add(requestLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
