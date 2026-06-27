using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;

public interface IPracticeSessionRepository
{
    Task AddAsync(PracticeSession session, CancellationToken ct);
    Task<PracticeSession?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<PracticeSession>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken ct);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct);
}
