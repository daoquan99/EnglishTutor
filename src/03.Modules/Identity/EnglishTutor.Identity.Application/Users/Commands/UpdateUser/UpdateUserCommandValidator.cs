using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(200);
    }
}
