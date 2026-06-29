namespace EnglishTutor.AiGateway.Contracts.Dtos;

public sealed class GenerateContentResult
{
    public ExecuteChatCompletionStatus Status { get; init; }
    public string? Text { get; init; }
    public string? Json { get; init; }
    public string? SchemaCode { get; init; }
    public int? SchemaVersion { get; init; }
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public long LatencyMs { get; init; }
    public string? ErrorCode { get; init; }
}
