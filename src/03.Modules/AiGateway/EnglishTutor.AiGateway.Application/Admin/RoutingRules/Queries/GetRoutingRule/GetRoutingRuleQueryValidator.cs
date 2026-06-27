using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Queries.GetRoutingRule;

public sealed class GetRoutingRuleQueryValidator : AbstractValidator<GetRoutingRuleQuery>
{
    public GetRoutingRuleQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Routing rule ID is required.");
    }
}
