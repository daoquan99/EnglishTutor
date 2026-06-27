using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.DisableProviderKey;

public sealed class DisableProviderKeyCommandValidator : AbstractValidator<DisableProviderKeyCommand>
{
    public DisableProviderKeyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Provider key ID is required.");
    }
}
