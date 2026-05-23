using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.SetUserRoles;

public sealed class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.RoleIds).NotNull();
        RuleForEach(command => command.RoleIds).NotEmpty();
    }
}
