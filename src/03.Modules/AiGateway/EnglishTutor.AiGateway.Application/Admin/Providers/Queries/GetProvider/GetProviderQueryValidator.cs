using FluentValidation;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Queries.GetProvider;

public sealed class GetProviderQueryValidator : AbstractValidator<GetProviderQuery>
{
    public GetProviderQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Provider ID is required.");
    }
}
