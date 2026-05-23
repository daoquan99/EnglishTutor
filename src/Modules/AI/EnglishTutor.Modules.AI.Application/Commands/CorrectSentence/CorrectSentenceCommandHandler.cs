using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;
using System.Text.Json;

namespace EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;

public sealed class CorrectSentenceCommandHandler(
    IAiClient aiClient,
    IAiRuntimeRouter runtimeRouter,
    IPromptBuilder promptBuilder,
    IAiRequestLogRepository aiRequestLogRepository,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CorrectSentenceCommand, SentenceCorrectionResult>
{
    public async Task<Result<SentenceCorrectionResult>> Handle(CorrectSentenceCommand request, CancellationToken cancellationToken)
    {
        var route = await runtimeRouter.ResolveAsync(
            AiTaskType.SentenceCorrection,
            AiCapabilityType.TextGeneration,
            cancellationToken);
        var prompt = await promptBuilder.BuildPromptAsync(
            "sentence_correction",
            new Dictionary<string, string>
            {
                ["original_text"] = request.OriginalText,
                ["native_language"] = request.LanguageContext.NativeLanguageCode,
                ["target_language"] = request.LanguageContext.TargetLanguageCode,
                ["explanation_language"] = request.LanguageContext.ExplanationLanguageCode,
                ["user_level"] = request.LanguageContext.UserLevel,
                ["topic"] = request.LanguageContext.Topic ?? string.Empty
            },
            cancellationToken);

        var response = await aiClient.SendAsync(
            new AiRequest(
                AiTaskType.SentenceCorrection,
                route.LegacyModel,
                prompt,
                route.MaxTokens,
                route.Temperature,
                route.PreferredProviderName,
                route.PreferredModelCode,
                route.Capability,
                route.PreferredProviderType,
                route.PreferredBaseUrl,
                route.PreferredApiKeySecretName),
            cancellationToken);

        var requestLog = AiRequestLog.Create(
            request.UserId,
            AiTaskType.SentenceCorrection,
            route.LegacyModel,
            response.PromptTokens,
            response.CompletionTokens,
            response.LatencyMs,
            AiRequestStatus.Success,
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(prompt))),
            null,
            dateTimeProvider.UtcNow);
        await aiRequestLogRepository.AddAsync(requestLog, cancellationToken);

        return ParseResponse(request.OriginalText, response.Text);
    }

    private static SentenceCorrectionResult ParseResponse(string originalText, string responseText)
    {
        if (TryParseJsonResponse(originalText, responseText, out var parsed))
        {
            return parsed;
        }

        return new SentenceCorrectionResult(
            originalText,
            string.IsNullOrWhiteSpace(responseText) ? originalText : responseText.Trim(),
            string.IsNullOrWhiteSpace(responseText) ? originalText : responseText.Trim(),
            0,
            0,
            "AI response was not structured; review the raw correction text.",
            []);
    }

    private static bool TryParseJsonResponse(string originalText, string responseText, out SentenceCorrectionResult result)
    {
        result = default!;
        try
        {
            using var document = JsonDocument.Parse(ExtractJson(responseText));
            var root = document.RootElement;
            var correctedText = GetString(root, "correctedText", "corrected_text", "correction");
            var naturalVersion = GetString(root, "naturalVersion", "natural_version");
            var feedback = GetString(root, "feedback", "explanation");

            if (string.IsNullOrWhiteSpace(correctedText))
            {
                return false;
            }

            result = new SentenceCorrectionResult(
                originalText,
                correctedText,
                string.IsNullOrWhiteSpace(naturalVersion) ? correctedText : naturalVersion,
                GetInt(root, "grammarScore", "grammar_score"),
                GetInt(root, "vocabularyScore", "vocabulary_score"),
                string.IsNullOrWhiteSpace(feedback) ? "Correction completed." : feedback,
                GetMistakes(root));
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string ExtractJson(string responseText)
    {
        var trimmed = responseText.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        return start >= 0 && end > start ? trimmed[start..(end + 1)] : trimmed;
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(element, name, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
        }

        return null;
    }

    private static int GetInt(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetProperty(element, name, out var property))
            {
                continue;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
            {
                return Math.Clamp(value, 0, 100);
            }

            if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out value))
            {
                return Math.Clamp(value, 0, 100);
            }
        }

        return 0;
    }

    private static IReadOnlyList<MistakeDetail> GetMistakes(JsonElement root)
    {
        if (!TryGetProperty(root, "mistakes", out var mistakesElement) || mistakesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return mistakesElement.EnumerateArray()
            .Where(element => element.ValueKind == JsonValueKind.Object)
            .Select(element => new MistakeDetail(
                GetString(element, "type") ?? "General",
                GetString(element, "original") ?? string.Empty,
                GetString(element, "corrected", "correction") ?? string.Empty,
                GetString(element, "explanation", "feedback") ?? string.Empty))
            .Where(mistake => !string.IsNullOrWhiteSpace(mistake.Original) || !string.IsNullOrWhiteSpace(mistake.Corrected))
            .ToArray();
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (candidate.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}
