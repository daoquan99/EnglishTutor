using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IModelRouter
{
    Task<AiModelType> ResolveModelAsync(AiTaskType taskType, CancellationToken cancellationToken);

    Task<ModelRoutingRule> GetRoutingRuleAsync(AiTaskType taskType, CancellationToken cancellationToken);
}
