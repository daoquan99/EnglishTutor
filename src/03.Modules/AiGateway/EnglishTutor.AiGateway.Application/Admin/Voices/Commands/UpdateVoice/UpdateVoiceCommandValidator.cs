using FluentValidation;

namespace EnglishTutor.AiGateway.Application.Admin.Voices.Commands.UpdateVoice;

public sealed class UpdateVoiceCommandValidator : AbstractValidator<UpdateVoiceCommand>
{
    public UpdateVoiceCommandValidator()
    {
        RuleFor(x => x.VoiceId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Style).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Gender).NotEmpty().MaximumLength(30);
    }
}
