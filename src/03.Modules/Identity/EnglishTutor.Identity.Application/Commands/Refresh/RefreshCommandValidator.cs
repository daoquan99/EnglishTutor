using FluentValidation;

namespace EnglishTutor.Identity.Application.Commands.Refresh;

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator() => RuleFor(c => c.RefreshToken).NotEmpty();
}
