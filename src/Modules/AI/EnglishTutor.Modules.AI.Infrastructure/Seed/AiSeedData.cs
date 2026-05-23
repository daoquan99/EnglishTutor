using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Infrastructure.Seed;

public static class AiSeedData
{
    public static IReadOnlyList<ModelRoutingRule> CreateDefaultRoutingRules() =>
        Enum.GetValues<AiTaskType>()
            .Select(CreateDefaultRoutingRule)
            .ToList();

    public static IReadOnlyList<PromptTemplate> CreateDefaultPromptTemplates(DateTime utcNow) =>
    [
        CreateSentenceCorrectionPromptTemplate(utcNow),
        CreateVocabularyExamplesPromptTemplate(utcNow)
    ];

    public static PromptTemplate? CreateDefaultPromptTemplate(string name, DateTime utcNow) =>
        name.Trim().ToLowerInvariant() switch
        {
            "sentence_correction" => CreateSentenceCorrectionPromptTemplate(utcNow),
            "vocabulary_examples" => CreateVocabularyExamplesPromptTemplate(utcNow),
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

    public static IReadOnlyList<AiProvider> CreateDefaultProviders()
    {
        var google = AiProvider.Register("google", "Google AI", AiProviderType.Google, null, "AI_PROVIDERS:GOOGLE:API_KEY", isEnabled: true);
        google.AddOrUpdateModel("gemini-flash", "Gemini Flash", AiCapabilityType.TextGeneration, true, 1_000_000, 8192, 0m, 0m, 10, true);
        google.AddOrUpdateModel("gemini-flash", "Gemini Flash", AiCapabilityType.ExerciseGeneration, true, 1_000_000, 8192, 0m, 0m, 10, true);
        google.AddOrUpdateModel("gemini-flash", "Gemini Flash", AiCapabilityType.LessonGeneration, true, 1_000_000, 8192, 0m, 0m, 10, true);
        google.AddOrUpdateModel("gemini-flash", "Gemini Flash", AiCapabilityType.Grading, true, 1_000_000, 8192, 0m, 0m, 20, true);
        google.AddOrUpdateModel("gemini-pro", "Gemini Pro", AiCapabilityType.Grading, true, 1_000_000, 8192, 0m, 0m, 10, true);
        google.AddOrUpdateModel("gemini-live", "Gemini Live", AiCapabilityType.LiveAudio, true, 128_000, 8192, 0m, 0m, 10, true);
        google.AddOrUpdateModel("gemini-tts", "Gemini TTS", AiCapabilityType.TextToSpeech, false, 32_000, 4096, 0m, 0m, 10, true);
        google.AddOrUpdateModel("gemini-flash", "Gemini Flash", AiCapabilityType.SpeechToText, true, 1_000_000, 8192, 0m, 0m, 20, true);
        google.AddOrUpdateModel("gemini-flash", "Gemini Flash", AiCapabilityType.PronunciationScoring, true, 1_000_000, 8192, 0m, 0m, 20, true);

        var local = AiProvider.Register("local", "Local / self-hosted models", AiProviderType.Local, null, null, isEnabled: true);
        local.AddOrUpdateModel("gemma", "Gemma", AiCapabilityType.TextGeneration, false, 8192, 2048, 0m, 0m, 20, true);
        local.AddOrUpdateModel("gemma", "Gemma", AiCapabilityType.ExerciseGeneration, false, 8192, 2048, 0m, 0m, 20, true);
        local.AddOrUpdateModel("gemma", "Gemma", AiCapabilityType.LessonGeneration, false, 8192, 2048, 0m, 0m, 20, true);
        local.AddOrUpdateModel("gemma", "Gemma", AiCapabilityType.Grading, false, 8192, 2048, 0m, 0m, 20, true);

        var openAi = AiProvider.Register("openai", "OpenAI", AiProviderType.OpenAI, null, "AI_PROVIDERS:OPENAI:API_KEY", isEnabled: false);
        openAi.AddOrUpdateModel("gpt-4o-mini", "GPT-4o mini", AiCapabilityType.TextGeneration, true, 128_000, 16_384, 0m, 0m, 30, false);
        openAi.AddOrUpdateModel("gpt-4o", "GPT-4o", AiCapabilityType.Grading, true, 128_000, 16_384, 0m, 0m, 30, false);

        var deepSeek = AiProvider.Register("deepseek", "DeepSeek", AiProviderType.DeepSeek, null, "AI_PROVIDERS:DEEPSEEK:API_KEY", isEnabled: false);
        deepSeek.AddOrUpdateModel("deepseek-v4-flash", "DeepSeek V4 Flash", AiCapabilityType.TextGeneration, true, 128_000, 8192, 0m, 0m, 40, false);
        deepSeek.AddOrUpdateModel("deepseek-v4-pro", "DeepSeek V4 Pro", AiCapabilityType.Grading, true, 128_000, 8192, 0m, 0m, 40, false);

        return [google, local, openAi, deepSeek];
    }

    public static IReadOnlyList<AiRuntimeRoute> CreateDefaultRuntimeRoutes()
    {
        return Enum.GetValues<AiTaskType>()
            .Select(CreateDefaultRuntimeRoute)
            .ToList();
    }

    public static AiRuntimeRoute CreateDefaultRuntimeRoute(AiTaskType taskType)
    {
        var legacyRule = CreateDefaultRoutingRule(taskType);
        var capability = ResolveCapability(taskType);
        var (preferredProvider, preferredModel) = ResolveProviderModel(legacyRule.PreferredModel);
        var (fallbackProvider, fallbackModel) = ResolveProviderModel(legacyRule.FallbackModel);

        if (taskType is AiTaskType.RealtimeVoice or AiTaskType.TextToSpeech)
        {
            fallbackProvider = null;
            fallbackModel = null;
        }

        return AiRuntimeRoute.Configure(
            taskType,
            capability,
            preferredProvider,
            preferredModel,
            fallbackProvider,
            fallbackModel,
            legacyRule.MaxTokens,
            legacyRule.Temperature);
    }

    public static AiCapabilityType ResolveCapability(AiTaskType taskType) =>
        taskType switch
        {
            AiTaskType.ExerciseGeneration => AiCapabilityType.ExerciseGeneration,
            AiTaskType.AssessmentGrading => AiCapabilityType.Grading,
            AiTaskType.RealtimeVoice => AiCapabilityType.LiveAudio,
            AiTaskType.TextToSpeech => AiCapabilityType.TextToSpeech,
            _ => AiCapabilityType.TextGeneration
        };

    private static (string ProviderName, string ModelCode) ResolveProviderModel(AiModelType modelType) =>
        modelType switch
        {
            AiModelType.Gemma => ("local", "gemma"),
            AiModelType.GeminiPro => ("google", "gemini-pro"),
            AiModelType.GeminiLive => ("google", "gemini-live"),
            AiModelType.GeminiTTS => ("google", "gemini-tts"),
            _ => ("google", "gemini-flash")
        };

    private static PromptTemplate CreateSentenceCorrectionPromptTemplate(DateTime utcNow)
    {
        var template = PromptTemplate.Create(
            "sentence_correction",
            AiTaskType.SentenceCorrection,
            "Correct learner sentences with language-aware feedback.",
            utcNow);

        template.AddVersion(
            "You are an English tutor. Explain feedback in {explanation_language}. Target language is {target_language}. Learner level is {user_level}.",
            "Correct this sentence and return JSON with correctedText, naturalVersion, grammarScore, vocabularyScore, feedback, and mistakes[{type,original,corrected,explanation}]. Topic: {topic}. Sentence: {original_text}",
            activate: true,
            utcNow: utcNow);

        return template;
    }

    private static PromptTemplate CreateVocabularyExamplesPromptTemplate(DateTime utcNow)
    {
        var template = PromptTemplate.Create(
            "vocabulary_examples",
            AiTaskType.VocabularyExampleGeneration,
            "Generate learner-friendly vocabulary examples.",
            utcNow);

        template.AddVersion(
            "You are an English vocabulary tutor. Target language is {target_language}. Learner level is {user_level}.",
            "Create one natural example sentence for '{word}' and keep it appropriate for the topic '{topic}'.",
            activate: true,
            utcNow: utcNow);

        return template;
    }
}
