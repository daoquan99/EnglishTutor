using FluentValidation;

namespace EnglishTutor.Identity.Application.Commands.PurgeExpiredRefreshTokens;

/// <summary>
/// Validator for PurgeExpiredRefreshTokensCommand.
/// </summary>
public sealed class PurgeExpiredRefreshTokensCommandValidator : AbstractValidator<PurgeExpiredRefreshTokensCommand>
{
    public PurgeExpiredRefreshTokensCommandValidator()
    {
        RuleFor(x => x.RetentionDays)
            .InclusiveBetween(1, 365)
            .WithMessage("RetentionDays must be between 1 and 365.");
    }
}
