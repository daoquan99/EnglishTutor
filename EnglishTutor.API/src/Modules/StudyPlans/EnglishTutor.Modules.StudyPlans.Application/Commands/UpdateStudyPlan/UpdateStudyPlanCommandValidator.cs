using FluentValidation;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateStudyPlan;

public sealed class UpdateStudyPlanCommandValidator : AbstractValidator<UpdateStudyPlanCommand>
{
    public UpdateStudyPlanCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.TargetLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(command => command.ReminderBeforeMinutes!.Value).InclusiveBetween(5, 60)
            .When(command => command.ReminderBeforeMinutes.HasValue);
        RuleFor(command => command.DailyTargetMinutes!.Value).InclusiveBetween(5, 240)
            .When(command => command.DailyTargetMinutes.HasValue);
        RuleFor(command => command.WeeklyTargetMinutes!.Value).InclusiveBetween(5, 240)
            .When(command => command.WeeklyTargetMinutes.HasValue);
        RuleFor(command => command.MonthlyTargetMinutes!.Value).InclusiveBetween(5, 240)
            .When(command => command.MonthlyTargetMinutes.HasValue);
        RuleFor(command => command.MonthlyTargetStudyDays!.Value).InclusiveBetween(1, 31)
            .When(command => command.MonthlyTargetStudyDays.HasValue);
    }
}
