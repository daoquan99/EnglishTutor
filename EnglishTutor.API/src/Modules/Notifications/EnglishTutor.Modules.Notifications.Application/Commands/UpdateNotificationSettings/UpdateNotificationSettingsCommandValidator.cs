using FluentValidation;

namespace EnglishTutor.Modules.Notifications.Application.Commands.UpdateNotificationSettings;

public sealed class UpdateNotificationSettingsCommandValidator : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.TimeZone).NotEmpty().MaximumLength(100);

        RuleFor(c => c.QuietHours).SetValidator(new QuietHoursInputValidator());

        RuleFor(c => c.StudyReminder).SetValidator(
            new ScheduleInputValidator("StudyReminder", requiresTime: true, hasBeforeMinutes: true));

        RuleFor(c => c.MissedStudyReminder).SetValidator(
            new ScheduleInputValidator("MissedStudyReminder", hasAfterMinutes: true));

        RuleFor(c => c.MistakeReviewReminder).SetValidator(
            new ScheduleInputValidator("MistakeReviewReminder", requiresTime: true, hasFrequency: true, allowedFrequencies: ["Daily", "Weekly", "BiWeekly"]));

        RuleFor(c => c.VocabularyReviewReminder).SetValidator(
            new ScheduleInputValidator("VocabularyReviewReminder", requiresTime: true, hasFrequency: true, allowedFrequencies: ["Daily", "Weekly", "BiWeekly"]));

        RuleFor(c => c.WeeklySummary).SetValidator(
            new ScheduleInputValidator("WeeklySummary", requiresTime: true, hasDayOfWeek: true));

        RuleFor(c => c.MonthlySummary).SetValidator(
            new ScheduleInputValidator("MonthlySummary", requiresTime: true, hasDayOfMonth: true));

        RuleFor(c => c.AssessmentReminder).SetValidator(
            new ScheduleInputValidator("AssessmentReminder", requiresTime: true));
    }
}

internal sealed class QuietHoursInputValidator : AbstractValidator<QuietHoursInput>
{
    public QuietHoursInputValidator()
    {
        When(q => q.Enabled, () =>
        {
            RuleFor(q => q.Start).NotEmpty().WithMessage("Quiet hours start time is required when enabled.");
            RuleFor(q => q.End).NotEmpty().WithMessage("Quiet hours end time is required when enabled.");
        });
    }
}

internal sealed class ScheduleInputValidator : AbstractValidator<ScheduleInput>
{
    private static readonly string[] ValidDaysOfWeek =
        Enum.GetNames<DayOfWeek>();

    public ScheduleInputValidator(
        string typeName,
        bool requiresTime = false,
        bool hasBeforeMinutes = false,
        bool hasAfterMinutes = false,
        bool hasFrequency = false,
        string[]? allowedFrequencies = null,
        bool hasDayOfWeek = false,
        bool hasDayOfMonth = false)
    {
        RuleFor(s => s.Channels)
            .Must(c => c.InApp || c.Email || c.Push)
            .When(s => s.Enabled)
            .WithMessage($"{typeName}: At least one delivery channel must be enabled.");

        if (requiresTime)
        {
            RuleFor(s => s.Time)
                .NotEmpty()
                .When(s => s.Enabled)
                .WithMessage($"{typeName}: Preferred time is required.");
        }

        if (hasBeforeMinutes)
        {
            RuleFor(s => s.BeforeMinutes)
                .InclusiveBetween(0, 180)
                .When(s => s.BeforeMinutes.HasValue)
                .WithMessage($"{typeName}: Reminder-before minutes must be between 0 and 180.");
        }

        if (hasAfterMinutes)
        {
            RuleFor(s => s.AfterMinutes)
                .InclusiveBetween(15, 1440)
                .When(s => s.AfterMinutes.HasValue)
                .WithMessage($"{typeName}: Remind-after minutes must be between 15 and 1440.");
        }

        if (hasFrequency && allowedFrequencies is not null)
        {
            RuleFor(s => s.Frequency)
                .Must(f => f is null || allowedFrequencies.Contains(f, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"{typeName}: Frequency must be one of: {string.Join(", ", allowedFrequencies)}.");
        }

        if (hasDayOfWeek)
        {
            RuleFor(s => s.DayOfWeek)
                .Must(d => d is null || ValidDaysOfWeek.Contains(d, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"{typeName}: Day of week is invalid.");

            RuleFor(s => s.DayOfWeek)
                .NotEmpty()
                .When(s => s.Enabled)
                .WithMessage($"{typeName}: Day of week is required.");
        }

        if (hasDayOfMonth)
        {
            RuleFor(s => s.DayOfMonth)
                .InclusiveBetween(1, 28)
                .When(s => s.DayOfMonth.HasValue)
                .WithMessage($"{typeName}: Day of month must be between 1 and 28.");

            RuleFor(s => s.DayOfMonth)
                .NotNull()
                .When(s => s.Enabled)
                .WithMessage($"{typeName}: Day of month is required.");
        }
    }
}
