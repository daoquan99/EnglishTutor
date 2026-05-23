using EnglishTutor.Modules.Speaking.Domain.Entities;

namespace EnglishTutor.Modules.Speaking.Application.Abstractions;

public interface ISpeakingSessionRepository
{
    Task AddAsync(SpeakingSession session, CancellationToken cancellationToken);

    Task<SpeakingSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<SpeakingSession>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken);
}

