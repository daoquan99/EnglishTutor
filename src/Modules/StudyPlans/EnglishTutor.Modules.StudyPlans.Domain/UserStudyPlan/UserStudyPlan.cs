using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;
using EnglishTutor.Modules.StudyPlans.Domain.Events;

namespace EnglishTutor.Modules.StudyPlans.Domain.Entities;

public sealed class UserStudyPlan : AggregateRoot<Guid>
{
    private readonly List<UserStudyWeekDay> _weekDays = [];
    private readonly List<StudyPlanTarget> _targets = [];

    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public TimeOnly PreferredStudyTime { get; private set; }
    public int ReminderBeforeMinutes { get; private set; }
    public string TimeZoneId { get; private set; } = string.Empty;
    public int DailyTargetMinutes { get; private set; }
    public int WeeklyTargetMinutes { get; private set; }
    public int MonthlyTargetMinutes { get; private set; }
    public int MonthlyTargetStudyDays { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<UserStudyWeekDay> WeekDays => _weekDays.AsReadOnly();
    public IReadOnlyCollection<StudyPlanTarget> Targets => _targets.AsReadOnly();

    private UserStudyPlan() { }

    public static UserStudyPlan Create(
        Guid userId,
        LanguageCode targetLanguageCode,
        TimeOnly preferredStudyTimeUtc,
        int reminderBeforeMinutes,
        string timeZoneId,
        int dailyTargetMinutes,
        int weeklyTargetMinutes,
        int monthlyTargetMinutes,
        int monthlyTargetStudyDays,
        IEnumerable<DayOfWeek>? studyDays,
        DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        var plan = new UserStudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language is required."),
            PreferredStudyTime = preferredStudyTimeUtc,
            ReminderBeforeMinutes = ValidateReminder(reminderBeforeMinutes),
            TimeZoneId = NormalizeRequired(timeZoneId, 100, "Time zone id"),
            DailyTargetMinutes = ValidateTargetMinutes(dailyTargetMinutes, "Daily target minutes"),
            WeeklyTargetMinutes = ValidateTargetMinutes(weeklyTargetMinutes, "Weekly target minutes"),
            MonthlyTargetMinutes = ValidateTargetMinutes(monthlyTargetMinutes, "Monthly target minutes"),
            MonthlyTargetStudyDays = ValidateTargetStudyDays(monthlyTargetStudyDays),
            IsActive = true,
            CreatedAtUtc = utcNow
        };

        plan.SetWeekDays(studyDays ?? DefaultStudyDays(), utcNow);
        plan.SetTargets(utcNow);
        plan.AddDomainEvent(new StudyPlanCreatedDomainEvent(userId, plan.Id, targetLanguageCode.Value, utcNow));

        return plan;
    }

    public void Update(
        TimeOnly preferredStudyTimeUtc,
        int reminderBeforeMinutes,
        int dailyTargetMinutes,
        int weeklyTargetMinutes,
        int monthlyTargetMinutes,
        int monthlyTargetStudyDays,
        DateTime utcNow)
    {
        PreferredStudyTime = preferredStudyTimeUtc;
        ReminderBeforeMinutes = ValidateReminder(reminderBeforeMinutes);
        DailyTargetMinutes = ValidateTargetMinutes(dailyTargetMinutes, "Daily target minutes");
        WeeklyTargetMinutes = ValidateTargetMinutes(weeklyTargetMinutes, "Weekly target minutes");
        MonthlyTargetMinutes = ValidateTargetMinutes(monthlyTargetMinutes, "Monthly target minutes");
        MonthlyTargetStudyDays = ValidateTargetStudyDays(monthlyTargetStudyDays);
        UpdatedAtUtc = utcNow;
        SetTargets(utcNow);
        AddDomainEvent(new StudyPlanUpdatedDomainEvent(UserId, Id, TargetLanguageCode.Value, utcNow));
    }

    public void UpdateSchedule(IEnumerable<DayOfWeek> studyDays, DateTime utcNow)
    {
        SetWeekDays(studyDays, utcNow);
        UpdatedAtUtc = utcNow;
        AddDomainEvent(new StudyPlanUpdatedDomainEvent(UserId, Id, TargetLanguageCode.Value, utcNow));
    }

    public static IReadOnlyCollection<DayOfWeek> DefaultStudyDays() =>
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    private void SetWeekDays(IEnumerable<DayOfWeek> studyDays, DateTime utcNow)
    {
        var selected = studyDays.Distinct().ToHashSet();
        if (selected.Count == 0)
        {
            throw new DomainException("At least one study day is required.");
        }

        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            var existing = _weekDays.SingleOrDefault(weekDay => weekDay.DayOfWeek == day);
            if (existing is null)
            {
                _weekDays.Add(UserStudyWeekDay.Create(Id, day, selected.Contains(day), utcNow));
            }
            else
            {
                existing.SetStudyDay(selected.Contains(day), utcNow);
            }
        }
    }

    private void SetTargets(DateTime utcNow)
    {
        UpsertTarget(TargetPeriod.Daily, DailyTargetMinutes, 1, utcNow);
        UpsertTarget(TargetPeriod.Weekly, WeeklyTargetMinutes, 0, utcNow);
        UpsertTarget(TargetPeriod.Monthly, MonthlyTargetMinutes, MonthlyTargetStudyDays, utcNow);
    }

    private void UpsertTarget(TargetPeriod period, int targetMinutes, int targetStudyDays, DateTime utcNow)
    {
        var target = _targets.SingleOrDefault(candidate => candidate.Period == period);
        if (target is null)
        {
            _targets.Add(StudyPlanTarget.Create(Id, period, targetMinutes, targetStudyDays, utcNow));
        }
        else
        {
            target.Update(targetMinutes, targetStudyDays, utcNow);
        }
    }

    private static int ValidateReminder(int value)
    {
        if (value is < 5 or > 60)
        {
            throw new DomainException("Reminder before minutes must be between 5 and 60.");
        }

        return value;
    }

    private static int ValidateTargetMinutes(int value, string fieldName)
    {
        if (value is < 5 or > 240)
        {
            throw new DomainException($"{fieldName} must be between 5 and 240.");
        }

        return value;
    }

    private static int ValidateTargetStudyDays(int value)
    {
        if (value is < 1 or > 31)
        {
            throw new DomainException("Monthly target study days must be between 1 and 31.");
        }

        return value;
    }

    private static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
