using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Commands.AppendPracticeMessage;

public sealed record AppendPracticeMessageCommand(
    Guid UserId,
    Guid SessionId,
    string Content) : ICommand<AppendTranscriptResult>;

public sealed class AppendPracticeMessageCommandValidator : AbstractValidator<AppendPracticeMessageCommand>
{
    public AppendPracticeMessageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
    }
}
