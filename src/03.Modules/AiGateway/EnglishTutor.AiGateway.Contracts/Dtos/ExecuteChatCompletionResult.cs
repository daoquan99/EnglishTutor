namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Result of the secure chat completion execution.
/// Contains only safe metadata and model responses, completely avoiding secrets.
/// </summary>
public class ExecuteChatCompletionResult
{
    public string? ResponseText { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public long LatencyMs { get; set; }
    public ExecuteChatCompletionStatus Status { get; set; }
    public string? ErrorCode { get; set; }
}
