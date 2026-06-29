namespace EnglishTutor.Practice.Contracts.Dtos;

public sealed record CreatePracticeLiveAccessRequest(
    string? VoiceId,
    string NativeLanguageCode = "vi",
    string TargetLanguageCode = "en");

public sealed record PracticeLiveAccessResult(
    string Status,
    string? EphemeralToken,
    string? ProviderModelId,
    string? Capability,
    string? VoiceId,
    DateTimeOffset? ExpiresAtUtc,
    string? ErrorCode);
