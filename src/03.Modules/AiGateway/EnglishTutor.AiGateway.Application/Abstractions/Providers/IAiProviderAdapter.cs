using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Application.Abstractions.Providers;

/// <summary>
/// Adapter for a single AI provider family (e.g. <c>openai</c>, <c>google</c>).
/// Receives the decrypted secret only at call time and must never log it.
/// </summary>
public interface IAiProviderAdapter
{
    /// <summary>Provider code this adapter handles (lower-case, matches <c>AiProvider.Code</c>).</summary>
    string ProviderCode { get; }

    Task<AiProviderAdapterResponse> ExecuteChatAsync(
        AiProviderAdapterRequest request,
        CancellationToken ct);
}

/// <summary>
/// Adapter call input. <see cref="Credential"/> is the decrypted provider
/// secret; it is passed only to the adapter at the last responsible moment and
/// is never persisted, returned, or logged.
/// </summary>
public sealed record AiProviderAdapterRequest(
    string ModelCode,
    string Credential,
    string SystemPrompt,
    string UserPrompt,
    bool? ThinkingEnabled = null,
    string? ResponseJsonSchema = null);

/// <summary>Adapter call output. Carries no secret material.</summary>
public sealed record AiProviderAdapterResponse(
    bool IsSuccess,
    string? ResponseText,
    int PromptTokens,
    int CompletionTokens,
    long LatencyMs,
    string? ErrorCode);
