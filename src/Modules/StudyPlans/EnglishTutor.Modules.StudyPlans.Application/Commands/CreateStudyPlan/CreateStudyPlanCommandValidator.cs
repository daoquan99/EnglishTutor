using EnglishTutor.Modules.StudyPlans.Application.Shared.Services;
using FluentValidation;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.CreateStudyPlan;

public sealed class CreateStudyPlanCommandValidator : AbstractValidator<CreateStudyPlanCommand>
{
    public CreateStudyPlanCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.TargetLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(command => command.TimeZoneId).Must(TimeZoneValidator.IsValid).WithMessage("Time zone id is invalid.");
        RuleFor(command => command.ReminderBeforeMinutes).InclusiveBetween(5, 60);
        RuleFor(command => command.DailyTargetMinutes).InclusiveBetween(5, 240);
        RuleFor(command => command.WeeklyTargetMinutes).InclusiveBetween(5, 240);
        RuleFor(command => command.MonthlyTargetMinutes).InclusiveBetween(5, 240);
        RuleFor(command => command.MonthlyTargetStudyDays).InclusiveBetween(1, 31);
        RuleFor(command => command.StudyDays).Must(days => days is null || days.Distinct().Any())
            .WithMessage("At least one study day is required.");
    }
}
