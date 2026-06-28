using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.CurrentPassword).NotEmpty().MaximumLength(200);
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}
