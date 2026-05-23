using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Infrastructure.Options;
using EnglishTutor.Modules.AI.Infrastructure.Secrets;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class GeminiClient(
    HttpClient httpClient,
    AiProviderOptions options,
    ISecretProvider secretProvider) : IAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken)
    {
        var configuredRequest = request with
        {
            ProviderName = string.IsNullOrWhiteSpace(request.ProviderName) ? "google" : request.ProviderName
        };
        var baseUrl = AiProviderClientConfiguration.ResolveBaseUrl(
            configuredRequest,
            options,
            "https://generativelanguage.googleapis.com/v1beta");
        var modelCode = AiProviderClientConfiguration.ResolveModelCode(
            configuredRequest,
            options,
            "gemini-2.5-flash");
        var apiKey = AiProviderClientConfiguration.ResolveApiKey(configuredRequest, options, secretProvider);

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = configuredRequest.Prompt }
                    }
                }
            },
            generationConfig = new
            {
                maxOutputTokens = configuredRequest.MaxTokens,
                temperature = Convert.ToDouble(configuredRequest.Temperature)
            }
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            AiProviderClientConfiguration.CombineUri(baseUrl, $"{NormalizeModelPath(modelCode)}:generateContent"));
        httpRequest.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);
        httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

        var stopwatch = Stopwatch.StartNew();
        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        stopwatch.Stop();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"AI provider '{configuredRequest.ProviderName}' returned {(int)response.StatusCode}. {ExtractProviderErrorSummary(responseJson)}");
        }

        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;
        var text = ExtractText(root);
        var (promptTokens, completionTokens) = ExtractUsage(root);

        return new AiResponse(text, promptTokens, completionTokens, stopwatch.ElapsedMilliseconds);
    }

    // Pull the upstream-provided message field only; never echo the entire response body — it
    // may include sensitive headers, account identifiers, or echoed prompt content.
    private static string ExtractProviderErrorSummary(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString() ?? string.Empty;
                }

                if (error.ValueKind == JsonValueKind.String)
                {
                    return error.GetString() ?? string.Empty;
                }
            }
        }
        catch (JsonException)
        {
            // Non-JSON error body; do not echo arbitrary text upstream returned.
        }

        return string.Empty;
    }

    private static string NormalizeModelPath(string modelCode) =>
        modelCode.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? modelCode
            : $"models/{modelCode}";

    private static string ExtractText(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini response does not contain candidates.");
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Gemini response does not contain content parts.");
        }

        return string.Concat(parts.EnumerateArray().Select(part =>
            part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String
                ? text.GetString()
                : string.Empty));
    }

    private static (int PromptTokens, int CompletionTokens) ExtractUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usageMetadata", out var usage))
        {
            return (0, 0);
        }

        var promptTokens = usage.TryGetProperty("promptTokenCount", out var promptTokensElement)
            ? promptTokensElement.GetInt32()
            : 0;
        var completionTokens = usage.TryGetProperty("candidatesTokenCount", out var completionTokensElement)
            ? completionTokensElement.GetInt32()
            : 0;

        return (promptTokens, completionTokens);
    }
}
