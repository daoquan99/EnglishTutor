using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Priority).InclusiveBetween(0, 1000);
        RuleForEach(c => c.PermissionCodes).NotEmpty().MaximumLength(200);
    }
}
