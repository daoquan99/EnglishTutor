using FluentValidation;

namespace EnglishTutor.AiGateway.Application.LiveAccess.Commands.CreateLiveAccessGrant;

public sealed class CreateLiveAccessGrantCommandValidator : AbstractValidator<CreateLiveAccessGrantCommand>
{
    public CreateLiveAccessGrantCommandValidator()
    {
        RuleFor(x => x.Request.LeaseId).NotEmpty();
        RuleFor(x => x.Request.Capability).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.NativeLanguageCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.TargetLanguageCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.VoiceId).MaximumLength(100);
    }
}
