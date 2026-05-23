using EnglishTutor.Modules.StudyPlans.Domain.Entities;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;

namespace EnglishTutor.Modules.StudyPlans.Application.Abstractions;

public interface IPlannedStudySessionRepository
{
    Task<PlannedStudySession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlannedStudySession>> ListAsync(Guid userId, DateTime? fromDateUtc, DateTime? toDateUtc, PlannedSessionStatus? status, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlannedStudySession>> ListPendingMissedCandidatesAsync(DateTime cutoffUtc, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid studyPlanId, DateTime scheduledDateUtc, CancellationToken cancellationToken);
    Task AddAsync(PlannedStudySession session, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<PlannedStudySession> sessions, CancellationToken cancellationToken);
    Task RemoveFuturePlannedSessionsAsync(Guid studyPlanId, DateTime fromUtc, CancellationToken cancellationToken);
    Task<bool> HasSessionForTypeOnDateAsync(Guid userId, string targetLanguageCode, DateOnly dateUtc, CancellationToken cancellationToken);
}
