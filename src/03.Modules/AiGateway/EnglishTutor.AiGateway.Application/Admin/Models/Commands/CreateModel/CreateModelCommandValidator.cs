using FluentValidation;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;

public sealed class CreateModelCommandValidator : AbstractValidator<CreateModelCommand>
{
    public CreateModelCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty().WithMessage("Provider ID is required.");
        RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Model display name is required.");
        RuleFor(x => x.Code).NotEmpty().WithMessage("Model code is required.");
        RuleFor(x => x.ProviderModelId).NotEmpty().WithMessage("Provider model ID is required.");
        RuleFor(x => x.Capabilities).NotEmpty().WithMessage("At least one capability is required.");
        RuleFor(x => x.Lifecycle).NotEmpty().WithMessage("Lifecycle is required.");
    }
}
