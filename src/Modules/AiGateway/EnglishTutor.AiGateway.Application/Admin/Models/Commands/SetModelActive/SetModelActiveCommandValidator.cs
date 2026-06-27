using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.SetModelActive;

public sealed class SetModelActiveCommandValidator : AbstractValidator<SetModelActiveCommand>
{
    public SetModelActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Model ID is required.");
    }
}
