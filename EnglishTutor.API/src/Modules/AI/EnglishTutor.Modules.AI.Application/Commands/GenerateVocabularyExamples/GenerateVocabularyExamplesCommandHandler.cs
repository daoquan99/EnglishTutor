using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Commands.GenerateVocabularyExamples;

public sealed class GenerateVocabularyExamplesCommandHandler(
    IAiClient aiClient,
    IAiRuntimeRouter runtimeRouter,
    IPromptBuilder promptBuilder)
    : ICommandHandler<GenerateVocabularyExamplesCommand, IReadOnlyList<VocabularyExampleResult>>
{
    public async Task<Result<IReadOnlyList<VocabularyExampleResult>>> Handle(
        GenerateVocabularyExamplesCommand request,
        CancellationToken cancellationToken)
    {
        var route = await runtimeRouter.ResolveAsync(
            AiTaskType.VocabularyExampleGeneration,
            AiCapabilityType.TextGeneration,
            cancellationToken);
        var prompt = await promptBuilder.BuildPromptAsync(
            "vocabulary_examples",
            new Dictionary<string, string>
            {
                ["word"] = request.Word,
                ["target_language"] = request.LanguageContext.TargetLanguageCode,
                ["explanation_language"] = request.LanguageContext.ExplanationLanguageCode,
                ["user_level"] = request.LanguageContext.UserLevel,
                ["topic"] = request.LanguageContext.Topic ?? string.Empty
            },
            cancellationToken);

        var response = await aiClient.SendAsync(
            new AiRequest(
                AiTaskType.VocabularyExampleGeneration,
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

        return new List<VocabularyExampleResult>
        {
            new(response.Text, string.Empty, string.Empty)
        };
    }
}
