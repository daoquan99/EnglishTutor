using EnglishTutor.BuildingBlocks.Infrastructure.Storage;
using EnglishTutor.Modules.Speaking.Application.Abstractions;

namespace EnglishTutor.Modules.Speaking.Infrastructure.Storage;

public sealed class SpeakingAudioStorage(IAudioStorageService audioStorageService) : ISpeakingAudioStorage
{
    public async Task<string> StoreTurnAudioAsync(
        Guid sessionId,
        int turnNumber,
        Stream audioStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".webm" : extension;
        var metadata = await audioStorageService.UploadAsync(
            audioStream,
            $"turn-{turnNumber}{safeExtension}",
            contentType,
            $"speaking/sessions/{sessionId}",
            cancellationToken);

        return metadata.Url;
    }
}
