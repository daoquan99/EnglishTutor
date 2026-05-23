using FluentValidation;

namespace EnglishTutor.Modules.AI.Application.Commands.ConfigureRuntimeRoute;

public sealed class ConfigureAiRuntimeRouteCommandValidator : AbstractValidator<ConfigureAiRuntimeRouteCommand>
{
    public ConfigureAiRuntimeRouteCommandValidator()
    {
        RuleFor(x => x.TaskType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Capability)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PreferredProviderName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PreferredModelCode)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.FallbackProviderName)
            .MaximumLength(200)
            .When(x => x.FallbackProviderName is not null);

        RuleFor(x => x.FallbackModelCode)
            .MaximumLength(200)
            .When(x => x.FallbackModelCode is not null);

        RuleFor(x => x.MaxTokens)
            .GreaterThan(0);

        RuleFor(x => x.Temperature)
            .InclusiveBetween(0, 2);
    }
}
