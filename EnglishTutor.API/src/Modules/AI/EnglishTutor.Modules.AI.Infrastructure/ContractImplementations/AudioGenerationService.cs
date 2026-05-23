using System.Security.Cryptography;
using System.Text;
using EnglishTutor.BuildingBlocks.Infrastructure.Storage;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;

namespace EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;

public sealed class AudioGenerationService(IAudioStorageService audioStorageService) : IAudioGenerationService
{
    public async Task<AudioGenerationResponse> GenerateAudioAsync(AudioGenerationRequest request, CancellationToken cancellationToken)
    {
        var normalizedText = string.Join(' ', request.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var languageCode = string.IsNullOrWhiteSpace(request.LanguageCode) ? "en" : request.LanguageCode.Trim().ToLowerInvariant();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{languageCode}:{request.Voice}:{normalizedText}")))[..24].ToLowerInvariant();
        var fileName = $"{hash}.mp3";
        var folder = $"tts/{languageCode}";
        var fileKey = $"{folder}/{fileName}";

        if (!await audioStorageService.ExistsAsync(fileKey, cancellationToken))
        {
            var bytes = Encoding.UTF8.GetBytes(normalizedText);
            await using var stream = new MemoryStream(bytes);
            await audioStorageService.UploadAsync(stream, fileName, "audio/mpeg", folder, cancellationToken);
        }

        var url = audioStorageService.GeneratePresignedUrl(fileKey, TimeSpan.FromHours(6));
        var duration = TimeSpan.FromMilliseconds(Math.Max(500, normalizedText.Length * 45));
        return new AudioGenerationResponse(url, "audio/mpeg", duration);
    }
}
