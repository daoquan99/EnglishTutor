using FluentValidation;

namespace EnglishTutor.Modules.Users.Application.Commands.CreateUserProfile;

public sealed class CreateUserProfileCommandValidator : AbstractValidator<CreateUserProfileCommand>
{
    public CreateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().Length(2, 100);
    }
}
