using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetUserOverviewReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAiUsageReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetLearningActivityReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetCommonMistakesReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAssessmentPassRatesReport;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetAuditLogs;
using EnglishTutor.Modules.AdminReports.Application.Queries.GetDeadLetters;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog.Enums;
using EnglishTutor.Modules.AdminReports.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EnglishTutor.Modules.AdminReports.Infrastructure.Persistence.Repositories;

public sealed class AdminReportQueryService(
    AdminReportsDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : IAdminReportQueryService
{
    private static readonly IReadOnlyDictionary<string, string> SourceSchemas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["auth"] = "auth",
        ["users"] = "users",
        ["studyplans"] = "studyplans",
        ["learningcontent"] = "learningcontent",
        ["vocabulary"] = "vocabulary",
        ["exercises"] = "exercises",
        ["speaking"] = "speaking",
        ["ai"] = "ai",
        ["mistakes"] = "mistakes",
        ["assessments"] = "assessments",
        ["progress"] = "progress",
        ["notifications"] = "notifications"
    };

    public async Task<IReadOnlyList<UserOverviewCardResponse>> GetUserOverviewAsync(int page, int pageSize, string? sortBy, CancellationToken cancellationToken)
    {
        var query = dbContext.UserOverviewCards.AsNoTracking();
        query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "lastactivity" => query.OrderByDescending(card => card.LastActivityAtUtc),
            "registeredat" => query.OrderByDescending(card => card.RegisteredAtUtc),
            _ => query.OrderByDescending(card => card.TotalExp)
        };

        return await query
            .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
            .Take(NormalizePageSize(pageSize))
            .Select(card => new UserOverviewCardResponse(
                card.UserId,
                card.Email,
                card.DisplayName,
                card.TargetLanguageCode,
                card.CurrentLevel,
                card.TotalExp,
                card.CurrentStreakDays,
                card.TotalSpeakingSessions,
                card.TotalExercisesCompleted,
                card.TotalVocabularyMastered,
                card.TotalMistakes,
                card.LastActivityAtUtc,
                card.RegisteredAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DailyAiUsageReportResponse>> GetAiUsageAsync(DateOnly? from, DateOnly? to, string? modelType, CancellationToken cancellationToken)
    {
        var query = dbContext.DailyAiUsageReports.AsNoTracking();
        if (from is not null)
        {
            query = query.Where(report => report.ReportDate >= from);
        }

        if (to is not null)
        {
            query = query.Where(report => report.ReportDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(modelType))
        {
            query = query.Where(report => report.ModelType == modelType.Trim());
        }

        return await query
            .OrderByDescending(report => report.ReportDate)
            .ThenBy(report => report.ModelType)
            .Select(report => new DailyAiUsageReportResponse(
                report.ReportDate,
                report.ModelType,
                report.TaskType,
                report.TotalRequests,
                report.TotalPromptTokens,
                report.TotalCompletionTokens,
                report.TotalTokens,
                report.AverageLatencyMs,
                report.FailedRequests,
                report.EstimatedCostUsd))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LearningActivityReportResponse>> GetLearningActivityAsync(DateOnly? from, DateOnly? to, string? period, CancellationToken cancellationToken)
    {
        var query = dbContext.LearningActivityReports.AsNoTracking();
        if (from is not null)
        {
            query = query.Where(report => report.ReportDate >= from);
        }

        if (to is not null)
        {
            query = query.Where(report => report.ReportDate <= to);
        }

        if (Enum.TryParse<ReportPeriod>(period, true, out var parsedPeriod))
        {
            query = query.Where(report => report.Period == parsedPeriod);
        }

        return await query
            .OrderByDescending(report => report.ReportDate)
            .Select(report => new LearningActivityReportResponse(
                report.ReportDate,
                report.Period.ToString(),
                report.TotalActiveUsers,
                report.TotalSpeakingSessions,
                report.TotalExercisesCompleted,
                report.TotalVocabularyReviews,
                report.TotalLessonsCompleted,
                report.TotalAssessments,
                report.TotalStudyMinutes))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CommonMistakeStatResponse>> GetCommonMistakesAsync(string? targetLanguageCode, int top, CancellationToken cancellationToken)
    {
        var query = dbContext.CommonMistakeStats.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            var normalized = targetLanguageCode.Trim().ToLowerInvariant();
            query = query.Where(stat => stat.TargetLanguageCode == normalized);
        }

        return await query
            .OrderByDescending(stat => stat.OccurrenceCount)
            .Take(Math.Clamp(top, 1, 100))
            .Select(stat => new CommonMistakeStatResponse(
                stat.TargetLanguageCode,
                stat.MistakeType,
                stat.Category,
                stat.OccurrenceCount,
                stat.AffectedUsers,
                stat.ExampleOriginal,
                stat.ExampleCorrected,
                stat.LastUpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssessmentPassRateReportResponse>> GetAssessmentPassRatesAsync(DateOnly? from, DateOnly? to, string? targetLanguageCode, CancellationToken cancellationToken)
    {
        var query = dbContext.AssessmentPassRateReports.AsNoTracking();
        if (from is not null)
        {
            query = query.Where(report => report.ReportDate >= from);
        }

        if (to is not null)
        {
            query = query.Where(report => report.ReportDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            var normalized = targetLanguageCode.Trim().ToLowerInvariant();
            query = query.Where(report => report.TargetLanguageCode == normalized);
        }

        return await query
            .OrderByDescending(report => report.ReportDate)
            .Select(report => new AssessmentPassRateReportResponse(
                report.TargetLanguageCode,
                report.AssessmentType,
                report.ForLevel,
                report.TotalAttempts,
                report.PassedCount,
                report.FailedCount,
                report.PassRate,
                report.AverageScore,
                report.Period.ToString(),
                report.ReportDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogResponse>> GetAuditLogsAsync(int page, int pageSize, string? action, string? targetEntity, CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsNoTracking();
        if (Enum.TryParse<AuditAction>(action, true, out var parsedAction))
        {
            query = query.Where(log => log.Action == parsedAction);
        }

        if (!string.IsNullOrWhiteSpace(targetEntity))
        {
            query = query.Where(log => log.TargetEntity == targetEntity.Trim());
        }

        return await query
            .OrderByDescending(log => log.CreatedAtUtc)
            .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
            .Take(NormalizePageSize(pageSize))
            .Select(log => new AuditLogResponse(
                log.Id,
                log.AdminUserId,
                log.Action.ToString(),
                log.TargetEntity,
                log.TargetEntityId,
                log.OldValue,
                log.NewValue,
                log.IpAddress,
                log.UserAgent,
                log.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeadLetterMessageResponse>> GetDeadLettersAsync(
        int page,
        int pageSize,
        string? sourceModule,
        string? eventType,
        string? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.DeadLetterMessages.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(sourceModule))
        {
            query = query.Where(message => message.SourceModule == sourceModule.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(message => message.EventType.Contains(eventType.Trim()));
        }

        if (Enum.TryParse<DeadLetterStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(message => message.Status == parsedStatus);
        }

        return await query
            .OrderByDescending(message => message.FailedAtUtc)
            .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
            .Take(NormalizePageSize(pageSize))
            .Select(message => new DeadLetterMessageResponse(
                message.Id,
                message.EventId,
                message.EventType,
                message.SourceModule,
                message.FailedAtUtc,
                message.RetryCount,
                message.LastError,
                message.StackTrace,
                message.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ReprocessDeadLetterAsync(Guid deadLetterId, CancellationToken cancellationToken)
    {
        var deadLetter = await dbContext.DeadLetterMessages.SingleOrDefaultAsync(message => message.Id == deadLetterId, cancellationToken);
        if (deadLetter is null || deadLetter.Status != DeadLetterStatus.Dead)
        {
            return false;
        }

        if (!SourceSchemas.TryGetValue(deadLetter.SourceModule, out var schema))
        {
            return false;
        }

        var sql = $"""
            INSERT INTO "{schema}"."OutboxMessages"
                ("Id", "EventId", "EventType", "Payload", "SourceModule", "Status", "RetryCount", "MaxRetryCount", "CreatedAtUtc")
            VALUES
                (@Id, @EventId, @EventType, @Payload, @SourceModule, @Status, @RetryCount, @MaxRetryCount, @CreatedAtUtc)
            """;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            sql,
            [
                new NpgsqlParameter("Id", Guid.NewGuid()),
                new NpgsqlParameter("EventId", deadLetter.EventId),
                new NpgsqlParameter("EventType", deadLetter.EventType),
                new NpgsqlParameter("Payload", deadLetter.Payload),
                new NpgsqlParameter("SourceModule", deadLetter.SourceModule),
                new NpgsqlParameter("Status", "Pending"),
                new NpgsqlParameter("RetryCount", (object)0),
                new NpgsqlParameter("MaxRetryCount", (object)5),
                new NpgsqlParameter("CreatedAtUtc", dateTimeProvider.UtcNow)
            ],
            cancellationToken);

        deadLetter.Status = DeadLetterStatus.Reprocessed;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static int NormalizePage(int page) => Math.Max(1, page);
    private static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, 100);
}
