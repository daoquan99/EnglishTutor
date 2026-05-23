namespace EnglishTutor.Modules.Speaking.Application.Abstractions;

public interface ISpeakingAudioStorage
{
    Task<string> StoreTurnAudioAsync(
        Guid sessionId,
        int turnNumber,
        Stream audioStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}
