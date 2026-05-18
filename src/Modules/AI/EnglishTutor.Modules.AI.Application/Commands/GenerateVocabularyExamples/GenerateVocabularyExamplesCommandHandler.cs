using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Commands.GenerateVocabularyExamples;

public sealed class GenerateVocabularyExamplesCommandHandler(
    IAiClient aiClient,
    IModelRouter modelRouter,
    IPromptBuilder promptBuilder)
    : ICommandHandler<GenerateVocabularyExamplesCommand, IReadOnlyList<VocabularyExampleResult>>
{
    public async Task<Result<IReadOnlyList<VocabularyExampleResult>>> Handle(
        GenerateVocabularyExamplesCommand request,
        CancellationToken cancellationToken)
    {
        var rule = await modelRouter.GetRoutingRuleAsync(AiTaskType.VocabularyExampleGeneration, cancellationToken);
        var model = rule.PreferredModel;
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
            new AiRequest(AiTaskType.VocabularyExampleGeneration, model, prompt, rule.MaxTokens, rule.Temperature),
            cancellationToken);

        return new List<VocabularyExampleResult>
        {
            new(response.Text, string.Empty, string.Empty)
        };
    }
}
