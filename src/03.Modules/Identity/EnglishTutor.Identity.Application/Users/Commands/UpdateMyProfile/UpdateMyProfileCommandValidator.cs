using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(200);
    }
}
