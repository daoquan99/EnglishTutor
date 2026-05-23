using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.DeleteRole;

public sealed class DeleteAuthRoleCommandValidator : AbstractValidator<DeleteAuthRoleCommand>
{
    public DeleteAuthRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty();
    }
}
