using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.StudyPlans.Application.Commands.CreateStudyPlan;
using EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateStudyPlan;
using EnglishTutor.Modules.StudyPlans.Domain.Entities;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;
using EnglishTutor.Modules.StudyPlans.Domain.Events;
using Xunit;

namespace EnglishTutor.Modules.StudyPlans.UnitTests;

public sealed class StudyPlansDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_Uses_Default_Monday_To_Friday_Study_Days()
    {
        var plan = CreatePlan(studyDays: null);

        Assert.Equal(5, plan.WeekDays.Count(day => day.IsStudyDay));
        Assert.DoesNotContain(plan.WeekDays, day => day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday && day.IsStudyDay);
    }

    [Fact]
    public void Create_Raises_StudyPlanCreated_Event()
    {
        var plan = CreatePlan([DayOfWeek.Monday]);

        Assert.Contains(plan.DomainEvents, item => item is StudyPlanCreatedDomainEvent);
    }

    [Fact]
    public void PlannedSession_MarkMissed_Raises_Event_And_Changes_Status()
    {
        var session = CreateSession();

        session.MarkMissed(UtcNow.AddHours(3));

        Assert.Equal(PlannedSessionStatus.Missed, session.Status);
        Assert.Contains(session.DomainEvents, item => item is PlannedStudySessionMissedDomainEvent);
    }

    [Fact]
    public void Completed_Session_Cannot_Be_Marked_Missed()
    {
        var session = CreateSession();
        session.Complete(UtcNow.AddHours(1));

        Assert.Throws<DomainException>(() => session.MarkMissed(UtcNow.AddHours(3)));
    }

    [Fact]
    public void Create_Rejects_Invalid_Reminder_Window()
    {
        Assert.Throws<DomainException>(() => UserStudyPlan.Create(
            Guid.NewGuid(),
            LanguageCode.English,
            new TimeOnly(13, 0),
            2,
            "Asia/Bangkok",
            30,
            120,
            240,
            20,
            [DayOfWeek.Monday],
            UtcNow));
    }

    [Fact]
    public void UpdateSchedule_Rejects_Empty_Study_Days()
    {
        var plan = CreatePlan([DayOfWeek.Monday]);

        Assert.Throws<DomainException>(() => plan.UpdateSchedule([], UtcNow.AddMinutes(1)));
    }

    [Fact]
    public void Update_Raises_StudyPlanUpdated_Event()
    {
        var plan = CreatePlan([DayOfWeek.Monday]);
        plan.ClearDomainEvents();

        plan.Update(new TimeOnly(14, 0), 15, 40, 160, 240, 20, UtcNow.AddMinutes(1));

        Assert.Contains(plan.DomainEvents, item => item is StudyPlanUpdatedDomainEvent);
    }

    [Fact]
    public void PlannedSession_Skip_Changes_Status()
    {
        var session = CreateSession();

        session.Skip(UtcNow.AddMinutes(5));

        Assert.Equal(PlannedSessionStatus.Skipped, session.Status);
    }

    [Fact]
    public void Skipped_Session_Cannot_Be_Completed()
    {
        var session = CreateSession();
        session.Skip(UtcNow.AddMinutes(5));

        Assert.Throws<DomainException>(() => session.Complete(UtcNow.AddMinutes(10)));
    }

    [Fact]
    public void CreateStudyPlanValidator_Rejects_Invalid_Edge_Values()
    {
        var validator = new CreateStudyPlanCommandValidator();

        var result = validator.Validate(new CreateStudyPlanCommand(
            Guid.Empty,
            "english",
            new TimeOnly(13, 0),
            1,
            "Invalid/Zone",
            0,
            500,
            0,
            40,
            []));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateStudyPlanCommand.UserId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateStudyPlanCommand.TargetLanguageCode));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateStudyPlanCommand.TimeZoneId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateStudyPlanCommand.StudyDays));
    }

    [Fact]
    public void UpdateStudyPlanValidator_Allows_Null_Optional_Updates_And_Rejects_Invalid_Targets()
    {
        var validator = new UpdateStudyPlanCommandValidator();

        var valid = validator.Validate(new UpdateStudyPlanCommand(
            Guid.NewGuid(),
            "en",
            null,
            null,
            null,
            null,
            null,
            null));

        var invalid = validator.Validate(new UpdateStudyPlanCommand(
            Guid.NewGuid(),
            "en",
            null,
            1,
            0,
            500,
            0,
            40));

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
    }

    private static UserStudyPlan CreatePlan(IReadOnlyCollection<DayOfWeek>? studyDays) =>
        UserStudyPlan.Create(
            Guid.NewGuid(),
            LanguageCode.English,
            new TimeOnly(13, 0),
            10,
            "Asia/Bangkok",
            30,
            120,
            240,
            20,
            studyDays,
            UtcNow);

    private static PlannedStudySession CreateSession() =>
        PlannedStudySession.Create(Guid.NewGuid(), Guid.NewGuid(), "en", UtcNow.AddHours(1), UtcNow);
}
