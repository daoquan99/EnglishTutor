using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.DeletePermission;

public sealed class DeleteAuthPermissionCommandValidator : AbstractValidator<DeleteAuthPermissionCommand>
{
    public DeleteAuthPermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty();
    }
}
