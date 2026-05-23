using FluentValidation;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateSchedule;

public sealed class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.TargetLanguageCode)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(command => command.Days)
            .NotEmpty();
    }
}
