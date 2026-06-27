using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Queries.GetRoutingRule;

internal sealed class GetRoutingRuleQueryHandler : IQueryHandler<GetRoutingRuleQuery, RoutingRuleView>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public GetRoutingRuleQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoutingRuleView>> Handle(GetRoutingRuleQuery query, CancellationToken ct)
    {
        var rule = await _unitOfWork.RoutingRules.GetByIdAsync(query.Id, ct);
        if (rule is null)
        {
            return Result.Failure<RoutingRuleView>(AiGatewayAdminErrors.NotFound("Routing rule"));
        }

        return Result.Success(MapRule(rule));
    }

    private static RoutingRuleView MapRule(AiRoutingRule r) => new(
        r.Id, r.Name, r.ActivityType, r.TopicCode, r.ScenarioCode, r.PrimaryModelId, r.FallbackModelId, r.IsActive);
}
