using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.CreateRole;

public sealed class CreateAuthRoleCommandValidator : AbstractValidator<CreateAuthRoleCommand>
{
    public CreateAuthRoleCommandValidator()
    {
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
