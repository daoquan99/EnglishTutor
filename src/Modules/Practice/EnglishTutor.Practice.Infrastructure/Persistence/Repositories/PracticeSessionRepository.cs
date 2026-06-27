using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Practice.Infrastructure.Persistence.Repositories;

public sealed class PracticeSessionRepository : IPracticeSessionRepository
{
    private readonly PracticeDbContext _context;

    public PracticeSessionRepository(PracticeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PracticeSession session, CancellationToken ct)
    {
        await _context.Sessions.AddAsync(session, ct);
    }

    public async Task<PracticeSession?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Sessions
            .Include(s => s.TranscriptMessages)
            .Include(s => s.SessionEvents)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<PracticeSession>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken ct)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Sessions
            .CountAsync(s => s.UserId == userId, ct);
    }
}
