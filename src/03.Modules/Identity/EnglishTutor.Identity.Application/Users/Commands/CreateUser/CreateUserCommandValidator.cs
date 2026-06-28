using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
        RuleFor(c => c.DisplayName).MaximumLength(200);
        RuleForEach(c => c.Roles).NotEmpty().MaximumLength(50);
    }
}
