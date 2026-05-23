using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Domain.Entities;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence.Repositories;

public sealed class PlannedStudySessionRepository(StudyPlansDbContext dbContext) : IPlannedStudySessionRepository
{
    public Task<PlannedStudySession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken) =>
        dbContext.PlannedStudySessions.SingleOrDefaultAsync(session => session.Id == sessionId, cancellationToken);

    public async Task<IReadOnlyList<PlannedStudySession>> ListAsync(
        Guid userId,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        PlannedSessionStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PlannedStudySessions.Where(session => session.UserId == userId);

        if (fromDateUtc.HasValue)
        {
            query = query.Where(session => session.ScheduledDateUtc >= fromDateUtc.Value);
        }

        if (toDateUtc.HasValue)
        {
            query = query.Where(session => session.ScheduledDateUtc <= toDateUtc.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(session => session.Status == status.Value);
        }

        return await query.OrderBy(session => session.ScheduledDateUtc).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlannedStudySession>> ListPendingMissedCandidatesAsync(
        DateTime cutoffUtc,
        CancellationToken cancellationToken) =>
        await dbContext.PlannedStudySessions
            .Where(session => session.Status == PlannedSessionStatus.Planned && session.ScheduledDateUtc < cutoffUtc)
            .OrderBy(session => session.ScheduledDateUtc)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid studyPlanId, DateTime scheduledDateUtc, CancellationToken cancellationToken) =>
        dbContext.PlannedStudySessions.AnyAsync(
            session => session.StudyPlanId == studyPlanId && session.ScheduledDateUtc == scheduledDateUtc,
            cancellationToken);

    public async Task AddAsync(PlannedStudySession session, CancellationToken cancellationToken) =>
        await dbContext.PlannedStudySessions.AddAsync(session, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<PlannedStudySession> sessions, CancellationToken cancellationToken) =>
        await dbContext.PlannedStudySessions.AddRangeAsync(sessions, cancellationToken);

    public async Task RemoveFuturePlannedSessionsAsync(Guid studyPlanId, DateTime fromUtc, CancellationToken cancellationToken)
    {
        var sessions = await dbContext.PlannedStudySessions
            .Where(session =>
                session.StudyPlanId == studyPlanId &&
                session.ScheduledDateUtc >= fromUtc &&
                session.Status == PlannedSessionStatus.Planned)
            .ToListAsync(cancellationToken);

        dbContext.PlannedStudySessions.RemoveRange(sessions);
    }

    public Task<bool> HasSessionForTypeOnDateAsync(
        Guid userId,
        string targetLanguageCode,
        DateOnly dateUtc,
        CancellationToken cancellationToken)
    {
        var normalizedLanguage = targetLanguageCode.Trim().ToLowerInvariant();
        var start = dateUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = dateUtc.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return dbContext.PlannedStudySessions.AnyAsync(
            session => session.UserId == userId &&
                session.TargetLanguageCode == normalizedLanguage &&
                session.ScheduledDateUtc >= start &&
                session.ScheduledDateUtc < end,
            cancellationToken);
    }
}
