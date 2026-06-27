using FluentValidation;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.UpdateProvider;

public sealed class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
{
    public UpdateProviderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Provider ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Provider name is required.");
    }
}
