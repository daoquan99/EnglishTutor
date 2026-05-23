using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
    }
}
