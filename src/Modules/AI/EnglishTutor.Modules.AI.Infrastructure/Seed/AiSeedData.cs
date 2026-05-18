using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Infrastructure.Seed;

public static class AiSeedData
{
    public static IReadOnlyList<ModelRoutingRule> CreateDefaultRoutingRules() =>
        Enum.GetValues<AiTaskType>()
            .Select(CreateDefaultRoutingRule)
            .ToList();

    public static IReadOnlyList<PromptTemplate> CreateDefaultPromptTemplates() =>
    [
        CreateSentenceCorrectionPromptTemplate(),
        CreateVocabularyExamplesPromptTemplate()
    ];

    public static PromptTemplate? CreateDefaultPromptTemplate(string name) =>
        name.Trim().ToLowerInvariant() switch
        {
            "sentence_correction" => CreateSentenceCorrectionPromptTemplate(),
            "vocabulary_examples" => CreateVocabularyExamplesPromptTemplate(),
            _ => null
        };

    public static ModelRoutingRule CreateDefaultRoutingRule(AiTaskType taskType) =>
        taskType switch
        {
            AiTaskType.RealtimeVoice => ModelRoutingRule.Create(taskType, AiModelType.GeminiLive, AiModelType.GeminiFlash, 1024, 0.3m),
            AiTaskType.TextToSpeech => ModelRoutingRule.Create(taskType, AiModelType.GeminiTTS, AiModelType.GeminiFlash, 1024, 0.2m),
            AiTaskType.AssessmentGrading => ModelRoutingRule.Create(taskType, AiModelType.GeminiPro, AiModelType.GeminiFlash, 2048, 0.1m),
            _ => ModelRoutingRule.Create(taskType, AiModelType.GeminiFlash, AiModelType.Gemma, 2048, 0.2m)
        };

    private static PromptTemplate CreateSentenceCorrectionPromptTemplate()
    {
        var template = PromptTemplate.Create(
            "sentence_correction",
            AiTaskType.SentenceCorrection,
            "Correct learner sentences with language-aware feedback.");

        template.AddVersion(
            "You are an English tutor. Explain feedback in {explanation_language}. Target language is {target_language}. Learner level is {user_level}.",
            "Correct this sentence and return concise feedback. Topic: {topic}. Sentence: {original_text}",
            activate: true);

        return template;
    }

    private static PromptTemplate CreateVocabularyExamplesPromptTemplate()
    {
        var template = PromptTemplate.Create(
            "vocabulary_examples",
            AiTaskType.VocabularyExampleGeneration,
            "Generate learner-friendly vocabulary examples.");

        template.AddVersion(
            "You are an English vocabulary tutor. Target language is {target_language}. Learner level is {user_level}.",
            "Create one natural example sentence for '{word}' and keep it appropriate for the topic '{topic}'.",
            activate: true);

        return template;
    }
}
