namespace EnglishTutor.AiGateway.Application.Abstractions.Live;

public sealed record AiLiveAccessRequest(
    Guid LeaseId,
    string Capability,
    string? VoiceId,
    string NativeLanguageCode,
    string TargetLanguageCode);

public sealed record AiLiveAccessResponse(
    bool IsSuccess,
    string? EphemeralToken,
    string? ProviderModelId,
    string? VoiceId,
    DateTimeOffset? ExpiresAtUtc,
    string? ErrorCode);

public interface IAiLiveAccessGateway
{
    Task<AiLiveAccessResponse> CreateAsync(
        AiLiveAccessRequest request,
        CancellationToken cancellationToken);
}
