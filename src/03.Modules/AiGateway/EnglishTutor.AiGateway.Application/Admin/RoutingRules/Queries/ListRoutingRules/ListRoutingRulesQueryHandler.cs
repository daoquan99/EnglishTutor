using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Queries.ListRoutingRules;

internal sealed class ListRoutingRulesQueryHandler : IQueryHandler<ListRoutingRulesQuery, IReadOnlyList<RoutingRuleView>>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public ListRoutingRulesQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<RoutingRuleView>>> Handle(ListRoutingRulesQuery query, CancellationToken ct)
    {
        var rules = await _unitOfWork.RoutingRules.ListAsync(ct);
        var views = rules.Select(MapRule).ToList();
        return Result.Success<IReadOnlyList<RoutingRuleView>>(views);
    }

    private static RoutingRuleView MapRule(AiRoutingRule r) => new(
        r.Id, r.Name, r.ActivityType, r.TopicCode, r.ScenarioCode, r.PrimaryModelId, r.FallbackModelId, r.IsActive);
}
