using System;

namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Request to securely execute an AI chat completion through the gateway using a lease.
/// </summary>
public class ExecuteChatCompletionRequest
{
    public Guid LeaseId { get; set; }
    public string SystemPrompt { get; set; } = default!;
    public string UserPrompt { get; set; } = default!;
    public string? ResponseJsonSchema { get; set; }
    public Guid? CorrelationId { get; set; }
}
