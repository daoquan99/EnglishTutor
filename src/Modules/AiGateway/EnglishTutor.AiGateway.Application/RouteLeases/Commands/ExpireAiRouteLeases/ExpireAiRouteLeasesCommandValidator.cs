using FluentValidation;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ExpireAiRouteLeases;

public sealed class ExpireAiRouteLeasesCommandValidator : AbstractValidator<ExpireAiRouteLeasesCommand>
{
    public ExpireAiRouteLeasesCommandValidator()
    {
        RuleFor(x => x.BatchSize)
            .GreaterThan(0)
            .WithMessage("Batch size must be greater than zero.");
    }
}
