using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence.Repositories;

public sealed class ModelRoutingRuleRepository(AiDbContext dbContext) : IModelRoutingRuleRepository
{
    public Task<ModelRoutingRule?> GetActiveByTaskTypeAsync(AiTaskType taskType, CancellationToken cancellationToken) =>
        dbContext.ModelRoutingRules.SingleOrDefaultAsync(rule => rule.TaskType == taskType && rule.IsActive, cancellationToken);
}
