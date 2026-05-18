using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;
using EnglishTutor.Modules.AI.Infrastructure.Prompts;
using EnglishTutor.Modules.AI.Infrastructure.Routing;
using Xunit;

namespace EnglishTutor.Modules.AI.UnitTests;

public sealed class AiCoreTests
{
    [Fact]
    public void ModelRoutingRule_Create_Requires_Positive_MaxTokens()
    {
        Assert.Throws<DomainException>(() =>
            ModelRoutingRule.Create(AiTaskType.SentenceCorrection, AiModelType.GeminiFlash, AiModelType.Gemma, 0, 0.2m));
    }

    [Fact]
    public void ModelRoutingRule_Create_Requires_Temperature_In_Range()
    {
        Assert.Throws<DomainException>(() =>
            ModelRoutingRule.Create(AiTaskType.SentenceCorrection, AiModelType.GeminiFlash, AiModelType.Gemma, 1000, 3m));
    }

    [Fact]
    public async Task ModelRouter_Uses_Db_Rule_When_Available()
    {
        var dbRule = ModelRoutingRule.Create(AiTaskType.RealtimeVoice, AiModelType.Gemma, AiModelType.GeminiFlash, 512, 0.1m);
        var router = new ModelRouter(new FakeRoutingRuleRepository(dbRule));

        Assert.Equal(AiModelType.Gemma, await router.ResolveModelAsync(AiTaskType.RealtimeVoice, CancellationToken.None));
    }

    [Fact]
    public async Task ModelRouter_Uses_Default_Rule_When_Db_Rule_Is_Missing()
    {
        var router = new ModelRouter(new FakeRoutingRuleRepository(null));

        Assert.Equal(AiModelType.GeminiLive, await router.ResolveModelAsync(AiTaskType.RealtimeVoice, CancellationToken.None));
    }

    [Fact]
    public async Task PromptBuilder_Includes_Language_Context()
    {
        var prompt = await new PromptBuilder(new FakePromptTemplateRepository(null)).BuildPromptAsync(
            "sentence_correction",
            new Dictionary<string, string>
            {
                ["explanation_language"] = "vi",
                ["target_language"] = "en",
                ["user_level"] = "A1",
                ["original_text"] = "I goes home",
                ["topic"] = ""
            },
            CancellationToken.None);

        Assert.Contains("en", prompt, StringComparison.Ordinal);
        Assert.Contains("A1", prompt, StringComparison.Ordinal);
        Assert.Contains("I goes home", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PromptBuilder_Uses_Db_Template_When_Available()
    {
        var template = PromptTemplate.Create("sentence_correction", AiTaskType.SentenceCorrection, "Test template");
        template.AddVersion("System {target_language}", "Sentence {original_text}", activate: true);

        var prompt = await new PromptBuilder(new FakePromptTemplateRepository(template)).BuildPromptAsync(
            "sentence_correction",
            new Dictionary<string, string>
            {
                ["target_language"] = "en",
                ["original_text"] = "I goes home"
            },
            CancellationToken.None);

        Assert.Contains("System en", prompt, StringComparison.Ordinal);
        Assert.Contains("Sentence I goes home", prompt, StringComparison.Ordinal);
    }

    private sealed class FakeRoutingRuleRepository(ModelRoutingRule? rule) : IModelRoutingRuleRepository
    {
        public Task<ModelRoutingRule?> GetActiveByTaskTypeAsync(AiTaskType taskType, CancellationToken cancellationToken) =>
            Task.FromResult(rule?.TaskType == taskType ? rule : null);
    }

    private sealed class FakePromptTemplateRepository(PromptTemplate? template) : IPromptTemplateRepository
    {
        public Task<PromptTemplate?> GetActiveByNameAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(template?.Name == name ? template : null);
    }
}
