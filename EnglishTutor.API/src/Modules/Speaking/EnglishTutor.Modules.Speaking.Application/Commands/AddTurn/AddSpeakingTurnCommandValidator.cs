using FluentValidation;

namespace EnglishTutor.Modules.Speaking.Application.Commands.AddTurn;

public sealed class AddSpeakingTurnCommandValidator : AbstractValidator<AddSpeakingTurnCommand>
{
    public AddSpeakingTurnCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.SessionId)
            .NotEmpty();

        RuleFor(command => command.UserText)
            .MaximumLength(4000)
            .When(command => command.UserText is not null);

        RuleFor(command => command.AudioFileName)
            .MaximumLength(255)
            .When(command => command.AudioFileName is not null);

        RuleFor(command => command)
            .Must(command => !string.IsNullOrWhiteSpace(command.UserText) || command.AudioFile is not null)
            .WithMessage("Either text or audio input is required.");
    }
}
