namespace EnglishTutor.AiGateway.Contracts.Dtos;

public enum CreateLiveAccessGrantStatus
{
    Success = 1,
    LeaseNotFound = 2,
    InvalidLeaseState = 3,
    CapabilityMismatch = 4,
    VoiceNotCompatible = 5,
    ProviderUnavailable = 6,
    ValidationError = 7
}

public sealed class CreateLiveAccessGrantResult
{
    public CreateLiveAccessGrantStatus Status { get; init; }
    public string? EphemeralToken { get; init; }
    public string? ProviderModelId { get; init; }
    public string? Capability { get; init; }
    public string? VoiceId { get; init; }
    public DateTimeOffset? ExpiresAtUtc { get; init; }
    public string? ErrorCode { get; init; }
}
