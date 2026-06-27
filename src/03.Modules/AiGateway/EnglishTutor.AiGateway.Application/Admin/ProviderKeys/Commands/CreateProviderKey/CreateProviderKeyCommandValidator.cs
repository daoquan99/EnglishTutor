using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey;

public sealed class CreateProviderKeyCommandValidator : AbstractValidator<CreateProviderKeyCommand>
{
    public CreateProviderKeyCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty().WithMessage("Provider ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Key name is required.");
        RuleFor(x => x.Secret).NotEmpty().WithMessage("Key secret is required.");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0).WithMessage("Priority must be greater than or equal to 0.");
    }
}
