using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.CreatePermission;

public sealed class CreateAuthPermissionCommandValidator : AbstractValidator<CreateAuthPermissionCommand>
{
    public CreateAuthPermissionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}
