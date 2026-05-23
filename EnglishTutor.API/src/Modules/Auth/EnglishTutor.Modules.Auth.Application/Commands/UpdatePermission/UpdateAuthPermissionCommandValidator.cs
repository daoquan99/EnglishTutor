using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdatePermission;

public sealed class UpdateAuthPermissionCommandValidator : AbstractValidator<UpdateAuthPermissionCommand>
{
    public UpdateAuthPermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}
