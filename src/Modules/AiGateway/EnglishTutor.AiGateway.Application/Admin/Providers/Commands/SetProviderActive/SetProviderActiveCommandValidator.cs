using FluentValidation;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.SetProviderActive;

public sealed class SetProviderActiveCommandValidator : AbstractValidator<SetProviderActiveCommand>
{
    public SetProviderActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Provider ID is required.");
    }
}
