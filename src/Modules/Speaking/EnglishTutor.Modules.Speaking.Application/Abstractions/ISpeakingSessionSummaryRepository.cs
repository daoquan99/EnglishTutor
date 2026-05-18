using EnglishTutor.Modules.Speaking.Domain.Entities;

namespace EnglishTutor.Modules.Speaking.Application.Abstractions;

public interface ISpeakingSessionSummaryRepository
{
    Task AddAsync(SpeakingSessionSummary summary, CancellationToken cancellationToken);

    Task<SpeakingSessionSummary?> GetBySessionIdAsync(Guid speakingSessionId, CancellationToken cancellationToken);
}
