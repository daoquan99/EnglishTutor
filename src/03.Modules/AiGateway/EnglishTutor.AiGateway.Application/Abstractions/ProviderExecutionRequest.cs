using System;

namespace EnglishTutor.AiGateway.Application.Abstractions;

/// <summary>
/// Application request contract for calling the provider execution gateway.
/// Contains only safe metadata, avoiding any exposure of encrypted or decrypted secrets.
/// </summary>
public class ProviderExecutionRequest
{
    public Guid LeaseId { get; set; }
    public string SystemPrompt { get; set; } = default!;
    public string UserPrompt { get; set; } = default!;
    public Guid? CorrelationId { get; set; }
}
