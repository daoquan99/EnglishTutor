using FluentValidation;

namespace EnglishTutor.Identity.Application.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MaximumLength(200);

        // Internal command fields populated by Presentation from HTTP headers.
        // Max-length rules prevent arbitrary long headers from hitting the
        // EF column max-length errors (DeviceId column is 100 chars,
        // DeviceName column is 200 chars, UserAgent is hashed not stored).
        RuleFor(c => c.DeviceId).MaximumLength(100);
        RuleFor(c => c.DeviceName).MaximumLength(200);
        RuleFor(c => c.UserAgent).MaximumLength(2000);
    }
}
