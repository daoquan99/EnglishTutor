using EnglishTutor.Modules.Speaking.Domain.Entities;

namespace EnglishTutor.Modules.Speaking.Application.Abstractions;

public interface ISpeakingTurnRepository
{
    Task AddResultAsync(SpeakingTurnResult result, CancellationToken cancellationToken);

    Task<IReadOnlyList<SpeakingTurnResult>> GetResultsBySessionIdAsync(Guid speakingSessionId, CancellationToken cancellationToken);
}
