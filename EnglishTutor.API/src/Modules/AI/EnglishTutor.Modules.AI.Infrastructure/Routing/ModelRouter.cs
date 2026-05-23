using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;
using EnglishTutor.Modules.AI.Infrastructure.Seed;

namespace EnglishTutor.Modules.AI.Infrastructure.Routing;

public sealed class ModelRouter(IModelRoutingRuleRepository routingRuleRepository) : IModelRouter
{
    public async Task<AiModelType> ResolveModelAsync(AiTaskType taskType, CancellationToken cancellationToken) =>
        (await GetRoutingRuleAsync(taskType, cancellationToken)).PreferredModel;

    public async Task<ModelRoutingRule> GetRoutingRuleAsync(AiTaskType taskType, CancellationToken cancellationToken)
    {
        var rule = await routingRuleRepository.GetActiveByTaskTypeAsync(taskType, cancellationToken);
        return rule ?? AiSeedData.CreateDefaultRoutingRule(taskType);
    }
}
