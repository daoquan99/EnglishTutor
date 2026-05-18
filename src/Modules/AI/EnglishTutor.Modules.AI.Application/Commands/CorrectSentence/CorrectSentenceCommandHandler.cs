using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;

public sealed class CorrectSentenceCommandHandler(
    IAiClient aiClient,
    IModelRouter modelRouter,
    IPromptBuilder promptBuilder,
    IAiRequestLogRepository aiRequestLogRepository)
    : ICommandHandler<CorrectSentenceCommand, SentenceCorrectionResult>
{
    public async Task<Result<SentenceCorrectionResult>> Handle(CorrectSentenceCommand request, CancellationToken cancellationToken)
    {
        var rule = await modelRouter.GetRoutingRuleAsync(AiTaskType.SentenceCorrection, cancellationToken);
        var model = rule.PreferredModel;
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
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(prompt))),
            null);
        await aiRequestLogRepository.AddAsync(requestLog, cancellationToken);

        return ParseResponse(request.OriginalText, response.Text);
    }

    private static SentenceCorrectionResult ParseResponse(string originalText, string responseText)
    {
        // Infrastructure adapters can later return structured JSON; until then keep a stable fallback contract.
        return new SentenceCorrectionResult(
            originalText,
            responseText,
            responseText,
            100,
            100,
            "No issues detected.",
            []);
    }
}
