namespace EnglishTutor.AiGateway.Contracts.Dtos;

public sealed class GenerateContentRequest
{
    public Guid LeaseId { get; init; }
    public string SystemInstruction { get; init; } = string.Empty;
    public string UserContent { get; init; } = string.Empty;
    public string? SchemaCode { get; init; }
    public int? SchemaVersion { get; init; }
    public string? ResponseJsonSchema { get; init; }
    public string NativeLanguageCode { get; init; } = "vi";
    public string TargetLanguageCode { get; init; } = "en";
    public string ExplanationLanguageCode { get; init; } = "vi";
    public Guid? CorrelationId { get; init; }
}
