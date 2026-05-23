using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;

public sealed class AssessmentGradingService(
    IAiClient aiClient,
    IAiRuntimeRouter runtimeRouter,
    IAiRequestLogRepository aiRequestLogRepository,
    IDateTimeProvider dateTimeProvider)
    : IAssessmentGradingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<GradingResponse> GradeAsync(GradingRequest request, CancellationToken cancellationToken)
    {
        var route = await runtimeRouter.ResolveAsync(
            AiTaskType.AssessmentGrading,
            AiCapabilityType.Grading,
            cancellationToken);

        var prompt = BuildPrompt(request);
        var response = await aiClient.SendAsync(
            new AiRequest(
                AiTaskType.AssessmentGrading,
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

        GradingResponse gradingResponse;
        try
        {
            gradingResponse = ParseGradingResponse(response.Text);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            await SaveRequestLogAsync(
                request.UserId,
                route.LegacyModel,
                response,
                prompt,
                AiRequestStatus.Failed,
                exception.Message,
                cancellationToken);

            throw new InvalidOperationException("AI grading response was not valid JSON.", exception);
        }

        await SaveRequestLogAsync(
            request.UserId,
            route.LegacyModel,
            response,
            prompt,
            AiRequestStatus.Success,
            null,
            cancellationToken);

        return gradingResponse;
    }

    private static string BuildPrompt(GradingRequest request) =>
        $$"""
        Grade the learner answer from 0 to 100.
        Target language: {{request.TargetLanguageCode}}
        User level: {{request.UserLevel}}
        Prompt: {{request.Prompt}}
        Learner answer: {{request.Answer}}
        Rubric: {{request.Rubric}}

        Return only valid JSON with this schema:
        {
          "score": 0,
          "feedback": "concise feedback for the learner",
          "rubricScores": {
            "overall": 0
          }
        }
        The score and every rubric score must be integers from 0 to 100.
        """;

    private async Task SaveRequestLogAsync(
        Guid userId,
        AiModelType model,
        AiResponse response,
        string prompt,
        AiRequestStatus status,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var requestLog = AiRequestLog.Create(
            userId,
            AiTaskType.AssessmentGrading,
            model,
            response.PromptTokens,
            response.CompletionTokens,
            response.LatencyMs,
            status,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(prompt))),
            errorMessage,
            dateTimeProvider.UtcNow);

        await aiRequestLogRepository.AddAsync(requestLog, cancellationToken);
    }

    private static GradingResponse ParseGradingResponse(string text)
    {
        var payload = JsonSerializer.Deserialize<AiGradingPayload>(ExtractJson(text), JsonOptions)
            ?? throw new InvalidOperationException("AI grading response was empty.");

        if (payload.Score is null or < 0 or > 100)
        {
            throw new InvalidOperationException("AI grading response must include score between 0 and 100.");
        }

        if (string.IsNullOrWhiteSpace(payload.Feedback))
        {
            throw new InvalidOperationException("AI grading response must include feedback.");
        }

        var rubricScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (payload.RubricScores is not null)
        {
            foreach (var pair in payload.RubricScores)
            {
                if (pair.Value is < 0 or > 100)
                {
                    throw new InvalidOperationException("AI grading response rubric scores must be between 0 and 100.");
                }

                rubricScores[pair.Key] = pair.Value;
            }
        }

        rubricScores["overall"] = payload.Score.Value;

        return new GradingResponse(payload.Score.Value, payload.Feedback.Trim(), rubricScores);
    }

    // Strip markdown code fences and locate the JSON object — Gemini commonly wraps replies in ```json … ```.
    private static string ExtractJson(string responseText)
    {
        var trimmed = responseText.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        return start >= 0 && end > start ? trimmed[start..(end + 1)] : trimmed;
    }

    private sealed record AiGradingPayload(
        int? Score,
        string? Feedback,
        Dictionary<string, int>? RubricScores);
}
