using FluentValidation;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ReleaseRouteLease;

public sealed class ReleaseRouteLeaseCommandValidator : AbstractValidator<ReleaseRouteLeaseCommand>
{
    public ReleaseRouteLeaseCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.LeaseId).NotEmpty().When(x => x.Request != null);
    }
}
