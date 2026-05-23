using FluentValidation;

namespace EnglishTutor.Modules.AdminReports.Application.Commands.ReprocessDeadLetter;

public sealed class ReprocessDeadLetterCommandValidator : AbstractValidator<ReprocessDeadLetterCommand>
{
    public ReprocessDeadLetterCommandValidator()
    {
        RuleFor(x => x.DeadLetterId).NotEmpty();
    }
}
