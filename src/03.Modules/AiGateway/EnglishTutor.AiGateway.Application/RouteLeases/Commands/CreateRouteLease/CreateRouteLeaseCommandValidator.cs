using FluentValidation;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.CreateRouteLease;

public sealed class CreateRouteLeaseCommandValidator : AbstractValidator<CreateRouteLeaseCommand>
{
    public CreateRouteLeaseCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.UserId).NotEmpty().When(x => x.Request != null);
        RuleFor(x => x.Request.ActivityType).NotEmpty().When(x => x.Request != null);
        RuleFor(x => x.Request.TopicCode).NotEmpty().When(x => x.Request != null);
        RuleFor(x => x.Request.ScenarioCode).NotEmpty().When(x => x.Request != null);
        RuleFor(x => x.Request.IdempotencyKey).NotEmpty().When(x => x.Request != null);
    }
}
