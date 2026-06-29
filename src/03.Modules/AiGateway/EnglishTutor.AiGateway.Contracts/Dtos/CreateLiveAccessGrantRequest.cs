namespace EnglishTutor.AiGateway.Contracts.Dtos;

public sealed class CreateLiveAccessGrantRequest
{
    public Guid LeaseId { get; init; }
    public string Capability { get; init; } = string.Empty;
    public string? VoiceId { get; init; }
    public string NativeLanguageCode { get; init; } = "vi";
    public string TargetLanguageCode { get; init; } = "en";
    public Guid? CorrelationId { get; init; }
}
