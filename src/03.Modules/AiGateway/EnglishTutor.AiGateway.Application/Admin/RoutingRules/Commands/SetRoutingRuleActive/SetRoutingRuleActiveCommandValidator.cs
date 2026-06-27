using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.SetRoutingRuleActive;

public sealed class SetRoutingRuleActiveCommandValidator : AbstractValidator<SetRoutingRuleActiveCommand>
{
    public SetRoutingRuleActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Routing rule ID is required.");
    }
}
