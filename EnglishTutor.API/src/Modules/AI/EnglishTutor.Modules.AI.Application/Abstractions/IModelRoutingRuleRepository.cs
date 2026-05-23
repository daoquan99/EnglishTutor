using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IModelRoutingRuleRepository
{
    Task<ModelRoutingRule?> GetActiveByTaskTypeAsync(AiTaskType taskType, CancellationToken cancellationToken);
}
