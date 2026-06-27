using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.CreateRoutingRule;

public sealed class CreateRoutingRuleCommandValidator : AbstractValidator<CreateRoutingRuleCommand>
{
    public CreateRoutingRuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Rule name is required.");
        RuleFor(x => x.ActivityType).NotEmpty().WithMessage("Activity type is required.");
        RuleFor(x => x.TopicCode).NotEmpty().WithMessage("Topic code is required.");
        RuleFor(x => x.ScenarioCode).NotEmpty().WithMessage("Scenario code is required.");
        RuleFor(x => x.PrimaryModelId).NotEmpty().WithMessage("Primary model ID is required.");
    }
}
