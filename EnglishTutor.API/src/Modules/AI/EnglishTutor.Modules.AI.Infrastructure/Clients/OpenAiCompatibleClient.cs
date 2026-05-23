using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;
using EnglishTutor.Modules.AI.Infrastructure.Options;
using EnglishTutor.Modules.AI.Infrastructure.Secrets;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class OpenAiCompatibleClient(
    HttpClient httpClient,
    AiProviderOptions options,
    ISecretProvider secretProvider) : IAiClient
{
    // Fallback defaults used only when no provider row / configuration is present.
    // Real deployments override these via AiProviderOptions.
#pragma warning disable S1075
    private const string DefaultBaseUrl = "https://api.openai.com/v1";
#pragma warning restore S1075
    private const string DefaultModelCode = "gpt-4o-mini";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken)
    {
        var providerName = string.IsNullOrWhiteSpace(request.ProviderName) ? "openai" : request.ProviderName;
        var configuredRequest = request with { ProviderName = providerName };
        var baseUrl = AiProviderClientConfiguration.ResolveBaseUrl(
            configuredRequest,
            options,
            DefaultBaseUrl);
        var modelCode = AiProviderClientConfiguration.ResolveModelCode(
            configuredRequest,
            options,
            DefaultModelCode);
        var apiKey = AiProviderClientConfiguration.ResolveApiKey(configuredRequest, options, secretProvider);

        var payload = new Dictionary<string, object?>
        {
            ["model"] = modelCode,
            ["messages"] = new[]
            {
                new ChatMessage("system", "You are EnglishTutor's AI provider adapter. Return concise, useful output for the application prompt."),
                new ChatMessage("user", configuredRequest.Prompt)
            },
            ["stream"] = false,
            ["temperature"] = Convert.ToDouble(configuredRequest.Temperature)
        };

        payload[ResolveMaxTokensProperty(configuredRequest)] = configuredRequest.MaxTokens;

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            AiProviderClientConfiguration.CombineUri(baseUrl, "chat/completions"));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
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

    // Pull only the upstream-provided message; never echo the entire response body.
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
            // Provider returned a non-JSON error body; fall through to empty string.
        }

        return string.Empty;
    }

    private static string ResolveMaxTokensProperty(AiRequest request) =>
        request.ProviderType == AiProviderType.OpenAI
            ? "max_completion_tokens"
            : "max_tokens";

    private static string ExtractText(JsonElement root)
    {
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("AI provider response does not contain choices.");
        }

        var choice = choices[0];
        if (!choice.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var content))
        {
            throw new InvalidOperationException("AI provider response does not contain message content.");
        }

        return content.ValueKind switch
        {
            JsonValueKind.String => content.GetString() ?? string.Empty,
            JsonValueKind.Array => string.Concat(content.EnumerateArray().Select(ExtractContentPartText)),
            JsonValueKind.Null => string.Empty,
            _ => content.ToString()
        };
    }

    private static string ExtractContentPartText(JsonElement part)
    {
        if (part.ValueKind == JsonValueKind.String)
        {
            return part.GetString() ?? string.Empty;
        }

        if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            return text.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static (int PromptTokens, int CompletionTokens) ExtractUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usage", out var usage))
        {
            return (0, 0);
        }

        var promptTokens = usage.TryGetProperty("prompt_tokens", out var promptTokensElement)
            ? promptTokensElement.GetInt32()
            : 0;
        var completionTokens = usage.TryGetProperty("completion_tokens", out var completionTokensElement)
            ? completionTokensElement.GetInt32()
            : 0;

        return (promptTokens, completionTokens);
    }

    private sealed record ChatMessage(string Role, string Content);
}
