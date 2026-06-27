using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Queries.ListProviderKeys;

public sealed class ListProviderKeysQueryValidator : AbstractValidator<ListProviderKeysQuery>
{
    public ListProviderKeysQueryValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty().WithMessage("Provider ID is required.");
    }
}
