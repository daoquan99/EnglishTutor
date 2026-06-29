using System.Diagnostics;
using System.Text.Json;
using EnglishTutor.AiGateway.Application.Abstractions.Providers;
using Google.GenAI;
using Google.GenAI.Types;

namespace EnglishTutor.AiGateway.Infrastructure.Providers;

internal sealed class GoogleProviderAdapter : IAiProviderAdapter
{
    public string ProviderCode => "google";

    public async Task<AiProviderAdapterResponse> ExecuteChatAsync(
        AiProviderAdapterRequest request,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var client = new Client(apiKey: request.Credential);
            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = [new Part { Text = request.SystemPrompt }]
                },
                ThinkingConfig = CreateThinkingConfig(request.ModelCode, request.ThinkingEnabled)
            };

            if (!string.IsNullOrWhiteSpace(request.ResponseJsonSchema))
            {
                config.ResponseMimeType = "application/json";
                config.ResponseJsonSchema = JsonSerializer.Deserialize<JsonElement>(request.ResponseJsonSchema);
            }

            var response = await client.Models.GenerateContentAsync(
                model: request.ModelCode,
                contents: request.UserPrompt,
                config: config,
                cancellationToken: ct);

            var text = response.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return Failure("provider.invalid_output", stopwatch.ElapsedMilliseconds);
            }

            if (request.ResponseJsonSchema is not null)
            {
                using var _ = JsonDocument.Parse(text);
            }

            return new AiProviderAdapterResponse(
                IsSuccess: true,
                ResponseText: text,
                PromptTokens: response.UsageMetadata?.PromptTokenCount ?? 0,
                CompletionTokens: response.UsageMetadata?.CandidatesTokenCount ?? 0,
                LatencyMs: stopwatch.ElapsedMilliseconds,
                ErrorCode: null);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Failure("provider.timeout", stopwatch.ElapsedMilliseconds);
        }
        catch (JsonException)
        {
            return Failure("provider.invalid_output", stopwatch.ElapsedMilliseconds);
        }
        catch (HttpRequestException)
        {
            return Failure("provider.unavailable", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            return Failure("provider.failed", stopwatch.ElapsedMilliseconds);
        }
    }

    private static ThinkingConfig? CreateThinkingConfig(string modelCode, bool? enabled)
    {
        if (enabled is null)
        {
            return null;
        }

        if (modelCode.Contains("3.1", StringComparison.OrdinalIgnoreCase))
        {
            return new ThinkingConfig
            {
                ThinkingLevel = enabled.Value ? ThinkingLevel.Low : ThinkingLevel.Minimal
            };
        }

        return new ThinkingConfig
        {
            ThinkingBudget = enabled.Value ? -1 : 0
        };
    }

    private static AiProviderAdapterResponse Failure(string code, long latencyMs) =>
        new(false, null, 0, 0, latencyMs, code);
}
