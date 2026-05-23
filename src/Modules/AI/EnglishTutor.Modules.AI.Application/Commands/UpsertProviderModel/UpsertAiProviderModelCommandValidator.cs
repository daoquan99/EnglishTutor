using FluentValidation;

namespace EnglishTutor.Modules.AI.Application.Commands.UpsertProviderModel;

public sealed class UpsertAiProviderModelCommandValidator : AbstractValidator<UpsertAiProviderModelCommand>
{
    public UpsertAiProviderModelCommandValidator()
    {
        RuleFor(x => x.ProviderName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ModelCode)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Capability)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.MaxInputTokens)
            .GreaterThan(0);

        RuleFor(x => x.MaxOutputTokens)
            .GreaterThan(0);

        RuleFor(x => x.CostPerInput1KTokens)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CostPerOutput1KTokens)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0);
    }
}
