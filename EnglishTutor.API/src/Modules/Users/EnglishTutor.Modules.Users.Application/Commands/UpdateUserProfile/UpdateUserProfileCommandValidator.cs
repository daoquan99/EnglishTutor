using FluentValidation;

namespace EnglishTutor.Modules.Users.Application.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(command => command.DisplayName).NotEmpty().Length(2, 100);
        RuleFor(command => command.AvatarUrl).MaximumLength(2048);
        RuleFor(command => command.Bio).MaximumLength(500);
    }
}
