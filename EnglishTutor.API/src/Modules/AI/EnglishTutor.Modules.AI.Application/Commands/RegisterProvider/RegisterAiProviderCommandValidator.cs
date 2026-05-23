using FluentValidation;

namespace EnglishTutor.Modules.AI.Application.Commands.RegisterProvider;

public sealed class RegisterAiProviderCommandValidator : AbstractValidator<RegisterAiProviderCommand>
{
    public RegisterAiProviderCommandValidator()
    {
        RuleFor(x => x.ProviderName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ProviderType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BaseUrl)
            .MaximumLength(2048)
            .When(x => x.BaseUrl is not null);

        RuleFor(x => x.ApiKeySecretName)
            .MaximumLength(200)
            .When(x => x.ApiKeySecretName is not null);
    }
}
