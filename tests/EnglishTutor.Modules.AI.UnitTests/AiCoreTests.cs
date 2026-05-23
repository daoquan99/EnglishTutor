using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Infrastructure.Storage;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;
using EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;
using EnglishTutor.Modules.AI.Infrastructure.Options;
using EnglishTutor.Modules.AI.Infrastructure.Prompts;
using EnglishTutor.Modules.AI.Infrastructure.Routing;
using EnglishTutor.Modules.AI.Infrastructure.Secrets;
using EnglishTutor.Modules.AI.Infrastructure.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace EnglishTutor.Modules.AI.UnitTests;

public sealed class AiCoreTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
    public void AiSeedData_Covers_All_Task_Types()
    {
        var rules = AiSeedData.CreateDefaultRoutingRules();

        Assert.Equal(Enum.GetValues<AiTaskType>().Length, rules.Count);
        Assert.All(Enum.GetValues<AiTaskType>(), taskType =>
            Assert.Contains(rules, rule => rule.TaskType == taskType));
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
        var prompt = await new PromptBuilder(new FakePromptTemplateRepository(null), new FakeDateTimeProvider()).BuildPromptAsync(
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
        var template = PromptTemplate.Create("sentence_correction", AiTaskType.SentenceCorrection, "Test template", UtcNow);
        template.AddVersion("System {target_language}", "Sentence {original_text}", activate: true, utcNow: UtcNow);

        var prompt = await new PromptBuilder(new FakePromptTemplateRepository(template), new FakeDateTimeProvider()).BuildPromptAsync(
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

    [Fact]
    public void AiSeedData_Provides_Default_Prompt_Templates()
    {
        var templates = AiSeedData.CreateDefaultPromptTemplates(UtcNow);

        Assert.Contains(templates, template => template.Name == "sentence_correction" && template.Versions.Any(version => version.IsActive));
        Assert.Contains(templates, template => template.Name == "vocabulary_examples" && template.Versions.Any(version => version.IsActive));
    }

    [Fact]
    public void AiSeedData_Runtime_Routes_Reference_Seeded_Provider_Model_Capabilities()
    {
        var providers = AiSeedData.CreateDefaultProviders();
        var routes = AiSeedData.CreateDefaultRuntimeRoutes();

        foreach (var route in routes)
        {
            var preferredProvider = Assert.Single(providers, provider => provider.ProviderName == route.PreferredProviderName);
            Assert.NotNull(preferredProvider.FindModel(route.PreferredModelCode, route.Capability));

            if (route.FallbackProviderName is not null && route.FallbackModelCode is not null)
            {
                var fallbackProvider = Assert.Single(providers, provider => provider.ProviderName == route.FallbackProviderName);
                Assert.NotNull(fallbackProvider.FindModel(route.FallbackModelCode, route.Capability));
            }
        }
    }

    [Fact]
    public void AiProvider_AddOrUpdateModel_Updates_Existing_Model_Capability()
    {
        var provider = AiProvider.Register("Google", "Google AI", AiProviderType.Google, null, null);

        var model = provider.AddOrUpdateModel(
            "Gemini-Flash",
            "Gemini Flash",
            AiCapabilityType.TextGeneration,
            supportsStreaming: true,
            maxInputTokens: 1000,
            maxOutputTokens: 500,
            costPerInput1KTokens: 0m,
            costPerOutput1KTokens: 0m,
            priority: 10,
            isEnabled: true);

        var updated = provider.AddOrUpdateModel(
            "gemini-flash",
            "Gemini Flash Updated",
            AiCapabilityType.TextGeneration,
            supportsStreaming: false,
            maxInputTokens: 2000,
            maxOutputTokens: 1000,
            costPerInput1KTokens: 1m,
            costPerOutput1KTokens: 2m,
            priority: 20,
            isEnabled: false);

        Assert.Same(model, updated);
        Assert.Single(provider.Models);
        Assert.Equal("gemini-flash", updated.ModelCode);
        Assert.Equal("Gemini Flash Updated", updated.DisplayName);
        Assert.False(updated.IsEnabled);
        Assert.Equal(20, updated.Priority);
    }

    [Fact]
    public void AiRuntimeRoute_Requires_Fallback_Provider_And_Model_Together()
    {
        Assert.Throws<DomainException>(() =>
            AiRuntimeRoute.Configure(
                AiTaskType.SentenceCorrection,
                AiCapabilityType.TextGeneration,
                "google",
                "gemini-flash",
                "local",
                null,
                2048,
                0.2m));
    }

    [Fact]
    public async Task AiRuntimeRouter_Uses_Active_Runtime_Route_When_Available()
    {
        var route = AiRuntimeRoute.Configure(
            AiTaskType.SentenceCorrection,
            AiCapabilityType.TextGeneration,
            "openai",
            "gpt-4o-mini",
            "google",
            "gemini-flash",
            4096,
            0.4m);

        var router = new AiRuntimeRouter(
            new FakeRuntimeRouteRepository(route),
            new FakeProviderRepository(),
            new FakeRoutingRuleRepository(null));

        var resolved = await router.ResolveAsync(
            AiTaskType.SentenceCorrection,
            AiCapabilityType.TextGeneration,
            CancellationToken.None);

        Assert.Equal("openai", resolved.PreferredProviderName);
        Assert.Equal("gpt-4o-mini", resolved.PreferredModelCode);
        Assert.Equal(AiProviderType.OpenAI, resolved.PreferredProviderType);
        Assert.Equal("google", resolved.FallbackProviderName);
        Assert.Equal(4096, resolved.MaxTokens);
    }

    [Fact]
    public async Task AiRuntimeRouter_Falls_Back_To_Legacy_Rule_When_Runtime_Route_Is_Missing()
    {
        var legacyRule = ModelRoutingRule.Create(
            AiTaskType.AssessmentGrading,
            AiModelType.GeminiPro,
            AiModelType.GeminiFlash,
            2048,
            0.1m);

        var router = new AiRuntimeRouter(
            new FakeRuntimeRouteRepository(),
            new FakeProviderRepository(),
            new FakeRoutingRuleRepository(legacyRule));

        var resolved = await router.ResolveAsync(
            AiTaskType.AssessmentGrading,
            AiCapabilityType.Grading,
            CancellationToken.None);

        Assert.Equal("google", resolved.PreferredProviderName);
        Assert.Equal("gemini-pro", resolved.PreferredModelCode);
        Assert.Equal(AiModelType.GeminiPro, resolved.LegacyModel);
    }

    [Fact]
    public void AiProviderOptions_Binds_Provider_Settings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AiProviders:OpenAI:BaseUrl"] = "https://api.openai.com/v1",
                ["AiProviders:OpenAI:ApiKeySecretName"] = "AI_PROVIDERS:OPENAI:API_KEY",
                ["AiProviders:OpenAI:DefaultModelCode"] = "gpt-4o-mini"
            })
            .Build();

        var options = AiProviderOptions.FromConfiguration(configuration);
        var openAi = options.FindProvider("openai");

        Assert.NotNull(openAi);
        Assert.Equal("https://api.openai.com/v1", openAi.BaseUrl);
        Assert.Equal("AI_PROVIDERS:OPENAI:API_KEY", openAi.ApiKeySecretName);
        Assert.Equal("gpt-4o-mini", openAi.DefaultModelCode);
    }

    [Fact]
    public void ConfigurationSecretProvider_Maps_Legacy_Ai_Secret_Names()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI_PROVIDERS:OPENAI:API_KEY"] = "secret"
            })
            .Build();

        var secretProvider = new ConfigurationSecretProvider(configuration);

        Assert.Equal("secret", secretProvider.GetRequiredSecret("AI__OpenAI__ApiKey"));
    }

    [Fact]
    public async Task AssessmentGradingService_Parses_Strict_Json_Response()
    {
        var logRepository = new CapturingAiRequestLogRepository();
        var service = new AssessmentGradingService(
            new FakeAiClient("""{"score":82,"feedback":"Good answer.","rubricScores":{"grammar":80}}"""),
            new StaticAiRuntimeRouter(),
            logRepository,
            new FakeDateTimeProvider());

        var response = await service.GradeAsync(
            new GradingRequest(Guid.NewGuid(), "en", "A1", "Prompt", "Answer", "Rubric"),
            CancellationToken.None);

        Assert.Equal(82, response.Score);
        Assert.Equal("Good answer.", response.Feedback);
        Assert.Equal(82, response.RubricScores["overall"]);
        Assert.Equal(80, response.RubricScores["grammar"]);
        Assert.Single(logRepository.Logs);
        Assert.Equal(AiRequestStatus.Success, logRepository.Logs[0].Status);
    }

    [Fact]
    public async Task AssessmentGradingService_Rejects_Non_Json_Response()
    {
        var logRepository = new CapturingAiRequestLogRepository();
        var service = new AssessmentGradingService(
            new FakeAiClient("I would score this 82 out of 100."),
            new StaticAiRuntimeRouter(),
            logRepository,
            new FakeDateTimeProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GradeAsync(new GradingRequest(Guid.NewGuid(), "en", "A1", "Prompt", "Answer", "Rubric"), CancellationToken.None));

        Assert.Single(logRepository.Logs);
        Assert.Equal(AiRequestStatus.Failed, logRepository.Logs[0].Status);
    }

    [Fact]
    public async Task PronunciationScoringService_Returns_Word_Level_Feedback_For_Sentences()
    {
        var service = new PronunciationScoringService();
        await using var audio = new MemoryStream([1, 2, 3]);

        var response = await service.ScoreAsync(
            new PronunciationScoringRequest(Guid.NewGuid(), "I am learning English", "en", audio, "audio/webm"),
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(response.WordLevelFeedbackJson));
        Assert.Contains("learning", response.WordLevelFeedbackJson, StringComparison.Ordinal);
        Assert.Contains("score", response.WordLevelFeedbackJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AudioGenerationService_Caches_Audio_By_Text_Hash()
    {
        var storage = new FakeAudioStorageService();
        var service = new AudioGenerationService(storage);

        var first = await service.GenerateAudioAsync(new AudioGenerationRequest("Hello world", "en", "default"), CancellationToken.None);
        var second = await service.GenerateAudioAsync(new AudioGenerationRequest("  Hello   world  ", "en", "default"), CancellationToken.None);

        Assert.Equal(first.AudioUrl, second.AudioUrl);
        Assert.Equal(1, storage.UploadCount);
        Assert.Single(storage.Keys);
        Assert.StartsWith("tts/en/", storage.Keys[0], StringComparison.Ordinal);
    }

    [Fact]
    public async Task LocalFileStorageService_Stores_And_Reads_File_From_Real_Local_Filesystem()
    {
        var root = Path.Combine(Path.GetTempPath(), "english-tutor-storage-tests", Guid.NewGuid().ToString("N"));
        var storage = new LocalFileStorageService(root);
        await using var upload = new MemoryStream([1, 2, 3, 4]);

        var metadata = await storage.UploadAsync(upload, "sample.webm", "audio/webm", "speaking/tests", CancellationToken.None);
        Assert.True(await storage.ExistsAsync(metadata.FileKey, CancellationToken.None));
        Assert.Equal("audio/webm", metadata.ContentType);

        await using (var download = await storage.DownloadAsync(metadata.FileKey, CancellationToken.None))
        {
            Assert.Equal(4, download.Length);
        }

        await storage.DeleteAsync(metadata.FileKey, CancellationToken.None);
        Assert.False(await storage.ExistsAsync(metadata.FileKey, CancellationToken.None));
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void S3FileStorageService_Generates_Presigned_Url_Without_Real_Aws_Client()
    {
        var storage = new S3FileStorageService(Options.Create(new StorageOptions
        {
            BucketName = "english-tutor",
            Region = "auto",
            AccessKey = "access",
            SecretKey = "secret",
            ServiceUrl = "https://r2.example.com"
        }));

        var url = storage.GeneratePresignedUrl("tts/en/sample.mp3", TimeSpan.FromMinutes(10));

        Assert.Contains("X-Amz-Signature=", url, StringComparison.Ordinal);
        Assert.Contains("X-Amz-Expires=600", url, StringComparison.Ordinal);
        Assert.Contains("/english-tutor/tts/en/sample.mp3", url, StringComparison.Ordinal);
    }

    private sealed class FakeRoutingRuleRepository(ModelRoutingRule? rule) : IModelRoutingRuleRepository
    {
        public Task<ModelRoutingRule?> GetActiveByTaskTypeAsync(AiTaskType taskType, CancellationToken cancellationToken) =>
            Task.FromResult(rule?.TaskType == taskType ? rule : null);
    }

    private sealed class FakeRuntimeRouteRepository(params AiRuntimeRoute[] routes) : IAiRuntimeRouteRepository
    {
        public Task<IReadOnlyList<AiRuntimeRoute>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AiRuntimeRoute>>(routes.ToList());

        public Task<AiRuntimeRoute?> GetByTaskTypeAsync(
            AiTaskType taskType,
            AiCapabilityType capability,
            CancellationToken cancellationToken) =>
            Task.FromResult(routes.SingleOrDefault(route => route.TaskType == taskType && route.Capability == capability));

        public Task<AiRuntimeRoute?> GetActiveByTaskTypeAsync(
            AiTaskType taskType,
            AiCapabilityType capability,
            CancellationToken cancellationToken) =>
            Task.FromResult(routes.SingleOrDefault(route =>
                route.TaskType == taskType &&
                route.Capability == capability &&
                route.IsActive));

        public Task AddAsync(AiRuntimeRoute route, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeProviderRepository(params AiProvider[] providers) : IAiProviderRepository
    {
        public Task<IReadOnlyList<AiProvider>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AiProvider>>(providers.ToList());

        public Task<AiProvider?> GetByNameAsync(string providerName, CancellationToken cancellationToken) =>
            Task.FromResult(providers.SingleOrDefault(provider => provider.ProviderName == providerName.Trim().ToLowerInvariant()));

        public Task AddAsync(AiProvider provider, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakePromptTemplateRepository(PromptTemplate? template) : IPromptTemplateRepository
    {
        public Task<PromptTemplate?> GetActiveByNameAsync(string name, CancellationToken cancellationToken) =>
            Task.FromResult(template?.Name == name ? template : null);
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => AiCoreTests.UtcNow;
    }

    private sealed class FakeAiClient(string responseText) : IAiClient
    {
        public Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new AiResponse(responseText, 10, 20, 30));
    }

    private sealed class StaticAiRuntimeRouter : IAiRuntimeRouter
    {
        public Task<AiRuntimeRouteResolution> ResolveAsync(
            AiTaskType taskType,
            AiCapabilityType capability,
            CancellationToken cancellationToken) =>
            Task.FromResult(new AiRuntimeRouteResolution(
                taskType,
                capability,
                "local",
                "gemma",
                AiProviderType.Local,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                1024,
                0.1m,
                AiModelType.Gemma));
    }

    private sealed class CapturingAiRequestLogRepository : IAiRequestLogRepository
    {
        public List<AiRequestLog> Logs { get; } = [];

        public Task AddAsync(AiRequestLog requestLog, CancellationToken cancellationToken)
        {
            Logs.Add(requestLog);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAudioStorageService : IAudioStorageService
    {
        private readonly HashSet<string> keys = [];

        public int UploadCount { get; private set; }
        public List<string> Keys { get; } = [];

        public Task<FileMetadata> UploadAsync(Stream file, string fileName, string contentType, string? folder, CancellationToken ct)
        {
            var fileKey = string.IsNullOrWhiteSpace(folder) ? fileName : $"{folder}/{fileName}";
            keys.Add(fileKey);
            Keys.Add(fileKey);
            UploadCount++;
            return Task.FromResult(new FileMetadata(fileKey, fileName, contentType, file.Length, GeneratePresignedUrl(fileKey, TimeSpan.FromHours(1)), UtcNow));
        }

        public Task<Stream> DownloadAsync(string fileKey, CancellationToken ct) =>
            Task.FromResult<Stream>(new MemoryStream([1]));

        public Task DeleteAsync(string fileKey, CancellationToken ct)
        {
            keys.Remove(fileKey);
            return Task.CompletedTask;
        }

        public string GeneratePresignedUrl(string fileKey, TimeSpan expiry) => $"https://storage.local/{fileKey}";

        public Task<bool> ExistsAsync(string fileKey, CancellationToken ct) => Task.FromResult(keys.Contains(fileKey));
    }
}
