using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.SetRolePermissions;

public sealed class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsCommandValidator()
    {
        RuleFor(c => c.RoleId).NotEmpty();
        RuleFor(c => c.PermissionCodes).NotNull();
        RuleForEach(c => c.PermissionCodes).NotEmpty().MaximumLength(200);
    }
}
