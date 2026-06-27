using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Application.Abstractions.Providers;

namespace EnglishTutor.AiGateway.Infrastructure.Providers;

/// <summary>
/// Deterministic in-process adapter used as the baseline provider and for tests.
/// It performs no external network calls and never logs the supplied secret; it
/// only validates that a non-empty decrypted key reached the adapter and echoes
/// a synthetic completion with token counts derived from the prompt lengths.
/// </summary>
internal sealed class MockProviderAdapter : IAiProviderAdapter
{
    public const string ProviderCodeValue = "mock";

    public string ProviderCode => ProviderCodeValue;

    public Task<AiProviderAdapterResponse> ExecuteChatAsync(
        AiProviderAdapterRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Credential))
        {
            return Task.FromResult(new AiProviderAdapterResponse(
                IsSuccess: false,
                ResponseText: null,
                PromptTokens: 0,
                CompletionTokens: 0,
                LatencyMs: 0,
                ErrorCode: "provider.key_missing"));
        }

        var promptTokens = EstimateTokens(request.SystemPrompt) + EstimateTokens(request.UserPrompt);
        var responseText = $"[mock:{request.ModelCode}] ack";
        var completionTokens = EstimateTokens(responseText);

        return Task.FromResult(new AiProviderAdapterResponse(
            IsSuccess: true,
            ResponseText: responseText,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens,
            LatencyMs: 1,
            ErrorCode: null));
    }

    private static int EstimateTokens(string? text) =>
        string.IsNullOrEmpty(text) ? 0 : Math.Max(1, text.Length / 4);
}
