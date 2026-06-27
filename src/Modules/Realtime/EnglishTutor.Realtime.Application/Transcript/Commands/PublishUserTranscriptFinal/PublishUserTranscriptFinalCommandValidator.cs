using FluentValidation;
using System;

namespace EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptFinal;

public sealed class PublishUserTranscriptFinalCommandValidator : AbstractValidator<PublishUserTranscriptFinalCommand>
{
    public PublishUserTranscriptFinalCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.SequenceNumber).GreaterThanOrEqualTo(0).WithMessage("SequenceNumber must be greater than or equal to 0.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content cannot be empty.");
    }
}
