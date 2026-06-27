using FluentValidation;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider;

public sealed class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Provider name is required.");
        RuleFor(x => x.Code).NotEmpty().WithMessage("Provider code is required.");
    }
}
