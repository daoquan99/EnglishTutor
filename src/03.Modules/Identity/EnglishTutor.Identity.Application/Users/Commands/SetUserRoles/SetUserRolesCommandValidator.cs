using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Roles).NotEmpty();
        RuleForEach(c => c.Roles).NotEmpty().MaximumLength(50);
    }
}
