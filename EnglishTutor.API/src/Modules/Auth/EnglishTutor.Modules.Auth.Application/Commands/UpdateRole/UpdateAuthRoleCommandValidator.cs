using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdateRole;

public sealed class UpdateAuthRoleCommandValidator : AbstractValidator<UpdateAuthRoleCommand>
{
    public UpdateAuthRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.PermissionIds)
            .NotNull();
    }
}
