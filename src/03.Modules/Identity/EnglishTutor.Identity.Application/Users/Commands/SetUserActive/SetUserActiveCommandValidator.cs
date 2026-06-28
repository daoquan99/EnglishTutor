using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.SetUserActive;

public sealed class SetUserActiveCommandValidator : AbstractValidator<SetUserActiveCommand>
{
    public SetUserActiveCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
    }
}
