using FluentValidation;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;

public sealed class CreateModelCommandValidator : AbstractValidator<CreateModelCommand>
{
    public CreateModelCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty().WithMessage("Provider ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Model name is required.");
        RuleFor(x => x.Code).NotEmpty().WithMessage("Model code is required.");
        RuleFor(x => x.Capabilities).NotNull().WithMessage("Capabilities cannot be null.");
    }
}
