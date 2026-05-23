using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IAiRuntimeRouteRepository
{
    Task<IReadOnlyList<AiRuntimeRoute>> ListAsync(CancellationToken cancellationToken);

    Task<AiRuntimeRoute?> GetByTaskTypeAsync(AiTaskType taskType, AiCapabilityType capability, CancellationToken cancellationToken);

    Task<AiRuntimeRoute?> GetActiveByTaskTypeAsync(AiTaskType taskType, AiCapabilityType capability, CancellationToken cancellationToken);

    Task AddAsync(AiRuntimeRoute route, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
