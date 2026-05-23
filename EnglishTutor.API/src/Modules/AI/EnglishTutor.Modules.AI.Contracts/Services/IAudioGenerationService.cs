using EnglishTutor.Modules.AI.Contracts.DTOs;

namespace EnglishTutor.Modules.AI.Contracts.Services;

public interface IAudioGenerationService
{
    Task<AudioGenerationResponse> GenerateAudioAsync(AudioGenerationRequest request, CancellationToken cancellationToken);
}
