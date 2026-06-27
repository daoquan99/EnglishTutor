using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.UpdateModel;

public sealed class UpdateModelCommandValidator : AbstractValidator<UpdateModelCommand>
{
    public UpdateModelCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Model ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Model name is required.");
        RuleFor(x => x.Capabilities).NotNull().WithMessage("Capabilities cannot be null.");
    }
}
