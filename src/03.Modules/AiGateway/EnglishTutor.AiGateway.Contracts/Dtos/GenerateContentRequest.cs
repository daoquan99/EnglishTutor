namespace EnglishTutor.AiGateway.Contracts.Dtos;

public sealed class GenerateContentRequest
{
    public Guid LeaseId { get; init; }
    public string SystemInstruction { get; init; } = string.Empty;
    public string UserContent { get; init; } = string.Empty;
    public string? SchemaCode { get; init; }
    public int? SchemaVersion { get; init; }
    public string? ResponseJsonSchema { get; init; }
    public Guid? CorrelationId { get; init; }
}
