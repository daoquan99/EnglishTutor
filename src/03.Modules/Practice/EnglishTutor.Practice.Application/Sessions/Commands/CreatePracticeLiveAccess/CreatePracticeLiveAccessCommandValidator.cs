using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Commands.CreatePracticeLiveAccess;

public sealed class CreatePracticeLiveAccessCommandValidator
    : AbstractValidator<CreatePracticeLiveAccessCommand>
{
    public CreatePracticeLiveAccessCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.NativeLanguageCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TargetLanguageCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.VoiceId).MaximumLength(100);
    }
}
