using System.Security.Cryptography;
using System.Text;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;

public sealed class EnglishCorrectionService(
    IAiClient aiClient,
    IModelRouter modelRouter,
    IPromptBuilder promptBuilder,
    IAiRequestLogRepository aiRequestLogRepository)
    : IEnglishCorrectionService
{
    public async Task<CorrectionResponse> CorrectSentenceAsync(CorrectionRequest request, CancellationToken cancellationToken)
    {
        var rule = await modelRouter.GetRoutingRuleAsync(AiTaskType.SentenceCorrection, cancellationToken);
        var model = rule.PreferredModel;
        var prompt = await promptBuilder.BuildPromptAsync(
            "sentence_correction",
            new Dictionary<string, string>
            {
                ["original_text"] = request.OriginalText,
                ["native_language"] = request.NativeLanguageCode,
                ["target_language"] = request.TargetLanguageCode,
                ["explanation_language"] = request.ExplanationLanguageCode,
                ["user_level"] = request.UserLevel,
                ["topic"] = request.Topic ?? string.Empty
            },
            cancellationToken);

        var response = await aiClient.SendAsync(
            new AiRequest(AiTaskType.SentenceCorrection, model, prompt, rule.MaxTokens, rule.Temperature),
            cancellationToken);

        var requestLog = AiRequestLog.Create(
            request.UserId,
            AiTaskType.SentenceCorrection,
            model,
            response.PromptTokens,
            response.CompletionTokens,
            response.LatencyMs,
            AiRequestStatus.Success,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(prompt))),
            null);

        await aiRequestLogRepository.AddAsync(requestLog, cancellationToken);

        return new CorrectionResponse(
            response.Text,
            response.Text,
            100,
            100,
            "No issues detected.",
            request.ExplanationLanguageCode,
            []);
    }
}
