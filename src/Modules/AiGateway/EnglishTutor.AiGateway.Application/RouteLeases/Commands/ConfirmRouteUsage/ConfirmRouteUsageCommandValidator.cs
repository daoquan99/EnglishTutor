using FluentValidation;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ConfirmRouteUsage;

public sealed class ConfirmRouteUsageCommandValidator : AbstractValidator<ConfirmRouteUsageCommand>
{
    public ConfirmRouteUsageCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.LeaseId).NotEmpty().When(x => x.Request != null);
        RuleFor(x => x.Request.PromptTokens).GreaterThanOrEqualTo(0).When(x => x.Request != null);
        RuleFor(x => x.Request.CompletionTokens).GreaterThanOrEqualTo(0).When(x => x.Request != null);
    }
}
