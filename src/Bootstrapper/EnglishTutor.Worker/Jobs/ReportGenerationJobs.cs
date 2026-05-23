using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.AdminReports.Contracts.IntegrationEvents;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog.Enums;
using EnglishTutor.Modules.AdminReports.Domain.Shared;
using EnglishTutor.Modules.AdminReports.Infrastructure.Persistence;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Enums;
using EnglishTutor.Modules.Assessments.Domain.Shared;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;
using EnglishTutor.Modules.Assessments.Infrastructure.Persistence;
using EnglishTutor.Modules.Progress.Domain.Entities;
using EnglishTutor.Modules.Progress.Domain.Enums;
using EnglishTutor.Modules.Progress.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace EnglishTutor.Worker.Jobs;

public sealed class WeeklyReportGenerationJob(
    ProgressDbContext progressDbContext,
    AssessmentsDbContext assessmentsDbContext,
    AdminReportsDbContext adminReportsDbContext,
    JsonSerializerService serializer,
    IDateTimeProvider dateTimeProvider,
    ILogger<WeeklyReportGenerationJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.Date);
        var start = today.AddDays(-6);
        await ReportGenerationRunner.RunAsync(
            start,
            today,
            ReportPeriod.Weekly,
            progressDbContext,
            assessmentsDbContext,
            adminReportsDbContext,
            serializer,
            dateTimeProvider.UtcNow,
            logger,
            context.CancellationToken);
    }
}

public sealed class MonthlyReportGenerationJob(
    ProgressDbContext progressDbContext,
    AssessmentsDbContext assessmentsDbContext,
    AdminReportsDbContext adminReportsDbContext,
    JsonSerializerService serializer,
    IDateTimeProvider dateTimeProvider,
    ILogger<MonthlyReportGenerationJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var now = dateTimeProvider.UtcNow;
        var firstDayThisMonth = new DateOnly(now.Year, now.Month, 1);
        var start = firstDayThisMonth.AddMonths(-1);
        var end = firstDayThisMonth.AddDays(-1);
        await ReportGenerationRunner.RunAsync(
            start,
            end,
            ReportPeriod.Monthly,
            progressDbContext,
            assessmentsDbContext,
            adminReportsDbContext,
            serializer,
            now,
            logger,
            context.CancellationToken);
    }
}

internal static class ReportGenerationRunner
{
    public static async Task RunAsync(
        DateOnly start,
        DateOnly end,
        ReportPeriod period,
        ProgressDbContext progressDbContext,
        AssessmentsDbContext assessmentsDbContext,
        AdminReportsDbContext adminReportsDbContext,
        JsonSerializerService serializer,
        DateTime utcNow,
        ILogger logger,
        CancellationToken ct)
    {
        var startUtc = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endExclusiveUtc = end.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var activities = await progressDbContext.LearningActivityLogs
            .AsNoTracking()
            .Where(activity => activity.CompletedAtUtc >= startUtc && activity.CompletedAtUtc < endExclusiveUtc)
            .Select(activity => new ReportActivityRow(
                activity.UserId,
                activity.ActivityType,
                activity.DurationSeconds,
                activity.ExpEarned))
            .ToListAsync(ct);

        await UpsertLearningActivityReportAsync(adminReportsDbContext, start, period, activities, ct);
        await UpsertAssessmentReportsAsync(adminReportsDbContext, assessmentsDbContext, start, period, startUtc, endExclusiveUtc, ct);
        await UpsertRetentionReportAsync(adminReportsDbContext, progressDbContext, start, endExclusiveUtc, period, activities, ct);
        EnqueueSummaryNotifications(adminReportsDbContext, serializer, period, start, end, activities, utcNow);

        await adminReportsDbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "{Period} report generated for {StartDate} to {EndDate}. ActiveUsers={ActiveUsers}",
            period,
            start,
            end,
            activities.Select(activity => activity.UserId).Distinct().Count());
    }

    private static async Task UpsertLearningActivityReportAsync(
        AdminReportsDbContext dbContext,
        DateOnly reportDate,
        ReportPeriod period,
        IReadOnlyCollection<ReportActivityRow> activities,
        CancellationToken ct)
    {
        var report = await dbContext.LearningActivityReports
            .SingleOrDefaultAsync(item => item.ReportDate == reportDate && item.Period == period, ct);

        var activeUsers = activities.Select(activity => activity.UserId).Distinct().Count();
        var speaking = activities.Count(activity => activity.ActivityType == ActivityType.SpeakingSessionCompleted);
        var exercises = activities.Count(activity => activity.ActivityType == ActivityType.ExerciseCompleted);
        var vocabulary = activities.Count(activity => activity.ActivityType is ActivityType.VocabularyReviewed or ActivityType.VocabularyPronunciationPracticed or ActivityType.ExampleSentencePronunciationPracticed);
        var lessons = activities.Count(activity => activity.ActivityType == ActivityType.LessonCompleted);
        var assessments = activities.Count(activity => activity.ActivityType is ActivityType.AssessmentCompleted or ActivityType.LevelUp);
        var studyMinutes = (int)Math.Round(activities.Sum(activity => activity.DurationSeconds) / 60m);

        if (report is null)
        {
            dbContext.LearningActivityReports.Add(LearningActivityReport.Create(reportDate, period, activeUsers, speaking, exercises, vocabulary, lessons, assessments, studyMinutes));
            return;
        }

        report.Update(activeUsers, speaking, exercises, vocabulary, lessons, assessments, studyMinutes);
    }

    private static async Task UpsertAssessmentReportsAsync(
        AdminReportsDbContext adminReportsDbContext,
        AssessmentsDbContext assessmentsDbContext,
        DateOnly reportDate,
        ReportPeriod period,
        DateTime startUtc,
        DateTime endExclusiveUtc,
        CancellationToken ct)
    {
        var attempts = await assessmentsDbContext.UserAssessmentAttempts
            .AsNoTracking()
            .Where(attempt =>
                attempt.GradedAtUtc >= startUtc &&
                attempt.GradedAtUtc < endExclusiveUtc &&
                (attempt.Status == AssessmentAttemptStatus.Passed || attempt.Status == AssessmentAttemptStatus.Failed))
            .ToListAsync(ct);

        foreach (var group in attempts.GroupBy(attempt => new { attempt.TargetLanguageCode, attempt.CurrentLevel }))
        {
            var total = group.Count();
            var passed = group.Count(attempt => attempt.Status == AssessmentAttemptStatus.Passed);
            var average = total == 0 ? 0 : group.Average(attempt => attempt.TotalScore ?? 0);

            var report = await adminReportsDbContext.AssessmentPassRateReports.SingleOrDefaultAsync(item =>
                item.ReportDate == reportDate &&
                item.Period == period &&
                item.TargetLanguageCode == group.Key.TargetLanguageCode &&
                item.AssessmentType == "LevelUpTest" &&
                item.ForLevel == group.Key.CurrentLevel,
                ct);

            if (report is null)
            {
                adminReportsDbContext.AssessmentPassRateReports.Add(AssessmentPassRateReport.Create(
                    reportDate,
                    period,
                    group.Key.TargetLanguageCode,
                    "LevelUpTest",
                    group.Key.CurrentLevel,
                    total,
                    passed,
                    (decimal)average));
                continue;
            }

            report.Update(total, passed, (decimal)average);
        }
    }

    private static async Task UpsertRetentionReportAsync(
        AdminReportsDbContext adminReportsDbContext,
        ProgressDbContext progressDbContext,
        DateOnly reportDate,
        DateTime currentEndExclusiveUtc,
        ReportPeriod period,
        IReadOnlyCollection<ReportActivityRow> currentActivities,
        CancellationToken ct)
    {
        var currentUsers = currentActivities.Select(activity => activity.UserId).Distinct().ToHashSet();
        var previousStartUtc = reportDate.AddDays(period == ReportPeriod.Weekly ? -7 : -DateTime.DaysInMonth(reportDate.AddMonths(-1).Year, reportDate.AddMonths(-1).Month))
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var previousEndUtc = reportDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var previousUsers = await progressDbContext.LearningActivityLogs
            .AsNoTracking()
            .Where(activity => activity.CompletedAtUtc >= previousStartUtc && activity.CompletedAtUtc < previousEndUtc)
            .Select(activity => activity.UserId)
            .Distinct()
            .ToListAsync(ct);

        var returningUsers = currentUsers.Intersect(previousUsers).Count();
        var report = await adminReportsDbContext.RetentionReports
            .SingleOrDefaultAsync(item => item.ReportDate == reportDate && item.Period == period, ct);

        if (report is null)
        {
            adminReportsDbContext.RetentionReports.Add(RetentionReport.Create(reportDate, period, currentUsers.Count, returningUsers));
            return;
        }

        report.Update(currentUsers.Count, returningUsers);
    }

    private static void EnqueueSummaryNotifications(
        AdminReportsDbContext adminReportsDbContext,
        JsonSerializerService serializer,
        ReportPeriod period,
        DateOnly start,
        DateOnly end,
        IReadOnlyCollection<ReportActivityRow> activities,
        DateTime utcNow)
    {
        foreach (var userActivities in activities.GroupBy(activity => activity.UserId))
        {
            var integrationEvent = new ProgressSummaryReadyIntegrationEvent(
                userActivities.Key,
                period.ToString(),
                start,
                end,
                userActivities.Count(),
                userActivities.Sum(activity => activity.ExpEarned),
                utcNow);

            adminReportsDbContext.OutboxMessages.Add(OutboxMessageFactory.Create(
                integrationEvent,
                "adminreports",
                serializer.Serialize(integrationEvent)));
        }
    }

    private sealed record ReportActivityRow(
        Guid UserId,
        ActivityType ActivityType,
        int DurationSeconds,
        int ExpEarned);
}
