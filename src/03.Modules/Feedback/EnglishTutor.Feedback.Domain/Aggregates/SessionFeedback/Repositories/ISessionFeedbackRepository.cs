using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Repositories;

public interface ISessionFeedbackRepository
{
    Task<SessionFeedback?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SessionFeedback?> GetByPracticeSessionIdAsync(Guid practiceSessionId, CancellationToken ct = default);
    Task AddAsync(SessionFeedback feedback, CancellationToken ct = default);
    void Update(SessionFeedback feedback);
}
