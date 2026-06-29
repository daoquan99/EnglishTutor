using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.UpdateModel;

public sealed class UpdateModelCommandValidator : AbstractValidator<UpdateModelCommand>
{
    public UpdateModelCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Model ID is required.");
        RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Model display name is required.");
        RuleFor(x => x.ProviderModelId).NotEmpty().WithMessage("Provider model ID is required.");
        RuleFor(x => x.Capabilities).NotEmpty().WithMessage("At least one capability is required.");
        RuleFor(x => x.Lifecycle).NotEmpty().WithMessage("Lifecycle is required.");
    }
}
