using FluentValidation;
using System;

namespace EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptPartial;

public sealed class PublishUserTranscriptPartialCommandValidator : AbstractValidator<PublishUserTranscriptPartialCommand>
{
    public PublishUserTranscriptPartialCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.SequenceNumber).GreaterThanOrEqualTo(0).WithMessage("SequenceNumber must be greater than or equal to 0.");
        RuleFor(x => x.Content).NotNull().WithMessage("Content cannot be null.");
    }
}
