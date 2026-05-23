using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence.Repositories;

public sealed class AiRuntimeRouteRepository(AiDbContext dbContext) : IAiRuntimeRouteRepository
{
    public async Task<IReadOnlyList<AiRuntimeRoute>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.AiRuntimeRoutes
            .OrderBy(route => route.TaskType)
            .ThenBy(route => route.Capability)
            .ToListAsync(cancellationToken);

    public Task<AiRuntimeRoute?> GetByTaskTypeAsync(
        AiTaskType taskType,
        AiCapabilityType capability,
        CancellationToken cancellationToken) =>
        dbContext.AiRuntimeRoutes.SingleOrDefaultAsync(
            route => route.TaskType == taskType && route.Capability == capability,
            cancellationToken);

    public Task<AiRuntimeRoute?> GetActiveByTaskTypeAsync(
        AiTaskType taskType,
        AiCapabilityType capability,
        CancellationToken cancellationToken) =>
        dbContext.AiRuntimeRoutes.SingleOrDefaultAsync(
            route => route.TaskType == taskType && route.Capability == capability && route.IsActive,
            cancellationToken);

    public async Task AddAsync(AiRuntimeRoute route, CancellationToken cancellationToken) =>
        await dbContext.AiRuntimeRoutes.AddAsync(route, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
