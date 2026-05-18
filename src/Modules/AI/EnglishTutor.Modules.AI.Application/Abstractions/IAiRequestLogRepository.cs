using EnglishTutor.Modules.AI.Domain.Entities;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IAiRequestLogRepository
{
    Task AddAsync(AiRequestLog requestLog, CancellationToken cancellationToken);
}
