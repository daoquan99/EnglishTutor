using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Feedback.Infrastructure.Persistence.Repositories;

internal sealed class SessionFeedbackRepository : ISessionFeedbackRepository
{
    private readonly FeedbackDbContext _context;

    public SessionFeedbackRepository(FeedbackDbContext context)
    {
        _context = context;
    }

    public Task<SessionFeedback?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.SessionFeedbacks
            .Include(f => f.Corrections)
            .Include(f => f.Vocabulary)
            .Include(f => f.MistakePatterns)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public Task<SessionFeedback?> GetByPracticeSessionIdAsync(Guid practiceSessionId, CancellationToken ct = default)
    {
        return _context.SessionFeedbacks
            .Include(f => f.Corrections)
            .Include(f => f.Vocabulary)
            .Include(f => f.MistakePatterns)
            .FirstOrDefaultAsync(f => f.PracticeSessionId == practiceSessionId, ct);
    }

    public async Task AddAsync(SessionFeedback feedback, CancellationToken ct = default)
    {
        await _context.SessionFeedbacks.AddAsync(feedback, ct);
    }

    public void Update(SessionFeedback feedback)
    {
        _context.SessionFeedbacks.Update(feedback);
    }
}
