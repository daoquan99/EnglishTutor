namespace EnglishTutor.AiGateway.Application.Abstractions;

/// <summary>
/// Application response contract returning safe execution outcomes and token usage counts.
/// </summary>
public class ProviderExecutionResponse
{
    public string? ResponseText { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public long LatencyMs { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorCode { get; set; }
}
