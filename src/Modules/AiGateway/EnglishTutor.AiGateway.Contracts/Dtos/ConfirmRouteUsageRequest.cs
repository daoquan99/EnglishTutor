using System;

namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Request to confirm AI route usage after a successful call.
/// </summary>
public class ConfirmRouteUsageRequest
{
    public Guid LeaseId { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public Guid? CorrelationId { get; set; }
}
