using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Api.Data;

public static class DatabaseSeeder
{
    public static async Task EnsureSchemaAndSeedAsync(IDbContextFactory<AppDbContext> dbFactory)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        // 1. Ensure all v8 Vocabulary, Daily Learning, AI Model Catalog, and Pronunciation tables exist in SQL Server
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID('dbo.Vocabularies', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Vocabularies] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [Word] nvarchar(160) NOT NULL,
                    [NormalizedWord] nvarchar(160) NOT NULL,
                    [LanguageCode] nvarchar(16) NOT NULL DEFAULT 'en',
                    [FrequencyRank] int NULL,
                    [SourceType] nvarchar(32) NOT NULL DEFAULT 'Seeded',
                    [SourceModelId] nvarchar(64) NULL,
                    [TopicCategory] nvarchar(128) NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NULL,
                    [IsActive] bit NOT NULL DEFAULT 1
                );
                CREATE INDEX [IX_Vocabularies_NormalizedWord_LanguageCode] ON [dbo].[Vocabularies] ([NormalizedWord], [LanguageCode]);
            END

            IF OBJECT_ID('dbo.VocabularySenses', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularySenses] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [VocabularyId] uniqueidentifier NOT NULL,
                    [PartOfSpeech] nvarchar(64) NOT NULL,
                    [DefinitionEn] nvarchar(1000) NOT NULL,
                    [MeaningVi] nvarchar(1000) NOT NULL,
                    [DescriptionEn] nvarchar(1500) NULL,
                    [CefrLevel] nvarchar(8) NOT NULL DEFAULT 'B1',
                    [IsPrimary] bit NOT NULL DEFAULT 1,
                    [DisplayOrder] int NOT NULL DEFAULT 1,
                    CONSTRAINT [FK_VocabularySenses_Vocabularies] FOREIGN KEY ([VocabularyId]) REFERENCES [dbo].[Vocabularies]([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_VocabularySenses_VocabularyId_DisplayOrder] ON [dbo].[VocabularySenses] ([VocabularyId], [DisplayOrder]);
            END

            IF OBJECT_ID('dbo.VocabularyPronunciations', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyPronunciations] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [VocabularyId] uniqueidentifier NOT NULL,
                    [Accent] nvarchar(16) NOT NULL,
                    [Ipa] nvarchar(160) NOT NULL,
                    [AudioUrl] nvarchar(512) NULL,
                    [AudioSource] nvarchar(64) NOT NULL DEFAULT 'BrowserTTS',
                    [SyllableCount] int NULL,
                    [StressPattern] nvarchar(64) NULL,
                    CONSTRAINT [FK_VocabularyPronunciations_Vocabularies] FOREIGN KEY ([VocabularyId]) REFERENCES [dbo].[Vocabularies]([Id]) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('dbo.VocabularyRelations', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyRelations] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [SenseId] uniqueidentifier NOT NULL,
                    [RelationType] nvarchar(32) NOT NULL,
                    [RelatedWord] nvarchar(160) NOT NULL,
                    [RelatedVocabularyId] uniqueidentifier NULL,
                    [RelatedSenseId] uniqueidentifier NULL,
                    [DisplayOrder] int NOT NULL DEFAULT 1,
                    CONSTRAINT [FK_VocabularyRelations_VocabularySenses] FOREIGN KEY ([SenseId]) REFERENCES [dbo].[VocabularySenses]([Id]) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('dbo.VocabularyWordFamilies', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyWordFamilies] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [VocabularyId] uniqueidentifier NOT NULL,
                    [RelatedWord] nvarchar(160) NOT NULL,
                    [PartOfSpeech] nvarchar(64) NOT NULL,
                    [RelatedVocabularyId] uniqueidentifier NULL,
                    CONSTRAINT [FK_VocabularyWordFamilies_Vocabularies] FOREIGN KEY ([VocabularyId]) REFERENCES [dbo].[Vocabularies]([Id]) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('dbo.VocabularyCollocations', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyCollocations] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [SenseId] uniqueidentifier NOT NULL,
                    [PhraseEn] nvarchar(300) NOT NULL,
                    [MeaningVi] nvarchar(300) NULL,
                    [DisplayOrder] int NOT NULL DEFAULT 1,
                    CONSTRAINT [FK_VocabularyCollocations_VocabularySenses] FOREIGN KEY ([SenseId]) REFERENCES [dbo].[VocabularySenses]([Id]) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('dbo.VocabularyExamples', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyExamples] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [SenseId] uniqueidentifier NOT NULL,
                    [SentenceEn] nvarchar(1000) NOT NULL,
                    [SentenceVi] nvarchar(1000) NULL,
                    [AudioUrl] nvarchar(512) NULL,
                    [CefrLevel] nvarchar(8) NULL,
                    [DisplayOrder] int NOT NULL DEFAULT 1,
                    CONSTRAINT [FK_VocabularyExamples_VocabularySenses] FOREIGN KEY ([SenseId]) REFERENCES [dbo].[VocabularySenses]([Id]) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('dbo.VocabularyGrammarPatterns', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyGrammarPatterns] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [SenseId] uniqueidentifier NOT NULL,
                    [Pattern] nvarchar(300) NOT NULL,
                    [Explanation] nvarchar(600) NULL,
                    [Example] nvarchar(600) NULL,
                    CONSTRAINT [FK_VocabularyGrammarPatterns_VocabularySenses] FOREIGN KEY ([SenseId]) REFERENCES [dbo].[VocabularySenses]([Id]) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('dbo.AiModelCatalog', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[AiModelCatalog] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [DisplayName] nvarchar(128) NOT NULL,
                    [ModelCode] nvarchar(128) NOT NULL,
                    [RoutedModelCode] nvarchar(128) NULL,
                    [SupportsTextGeneration] bit NOT NULL,
                    [SupportsStructuredOutput] bit NOT NULL,
                    [SupportsAudioInput] bit NOT NULL,
                    [SupportsPronunciationAssessment] bit NOT NULL,
                    [SupportsLiveAudio] bit NOT NULL,
                    [SupportsTranscription] bit NOT NULL,
                    [SupportsTranslation] bit NOT NULL,
                    [SupportsTts] bit NOT NULL,
                    [SupportsStreaming] bit NOT NULL,
                    [IsAvailable] bit NOT NULL DEFAULT 1,
                    [IsDeprecated] bit NOT NULL DEFAULT 0,
                    [IsEnabled] bit NOT NULL DEFAULT 1,
                    [CapabilityNote] nvarchar(500) NULL,
                    [DisplayOrder] int NOT NULL DEFAULT 1
                );
                CREATE UNIQUE INDEX [IX_AiModelCatalog_ModelCode] ON [dbo].[AiModelCatalog] ([ModelCode]);
            END

            IF OBJECT_ID('dbo.VocabularyGenerationAudits', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyGenerationAudits] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [UserId] nvarchar(64) NOT NULL,
                    [ModelCode] nvarchar(128) NOT NULL,
                    [PromptVersion] nvarchar(64) NOT NULL,
                    [RequestedCount] int NOT NULL,
                    [AcceptedCount] int NOT NULL,
                    [RejectedCount] int NOT NULL,
                    [Status] nvarchar(32) NOT NULL,
                    [ErrorMessage] nvarchar(max) NULL,
                    [CreatedAt] datetimeoffset NOT NULL
                );
            END

            IF OBJECT_ID('dbo.DailyVocabularyPlans', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[DailyVocabularyPlans] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [UserId] nvarchar(64) NOT NULL,
                    [LearningDate] nvarchar(16) NOT NULL,
                    [TimeZoneId] nvarchar(64) NOT NULL DEFAULT 'Asia/Ho_Chi_Minh',
                    [NewVocabularyTarget] int NOT NULL DEFAULT 20,
                    [Status] nvarchar(32) NOT NULL DEFAULT 'Active',
                    [CreatedAt] datetimeoffset NOT NULL,
                    [CompletedAt] datetimeoffset NULL
                );
                CREATE UNIQUE INDEX [IX_DailyVocabularyPlans_UserId_LearningDate] ON [dbo].[DailyVocabularyPlans] ([UserId], [LearningDate]);
            END

            IF OBJECT_ID('dbo.DailyVocabularyItems', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[DailyVocabularyItems] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [PlanId] uniqueidentifier NOT NULL,
                    [VocabularyId] uniqueidentifier NOT NULL,
                    [SenseId] uniqueidentifier NOT NULL,
                    [ItemType] nvarchar(32) NOT NULL,
                    [Status] nvarchar(32) NOT NULL,
                    [AssignedAt] datetimeoffset NOT NULL,
                    [CompletedAt] datetimeoffset NULL,
                    [DisplayOrder] int NOT NULL,
                    CONSTRAINT [FK_DailyVocabularyItems_DailyVocabularyPlans] FOREIGN KEY ([PlanId]) REFERENCES [dbo].[DailyVocabularyPlans]([Id]) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX [IX_DailyVocabularyItems_PlanId_SenseId] ON [dbo].[DailyVocabularyItems] ([PlanId], [SenseId]);
            END

            IF OBJECT_ID('dbo.UserVocabularyProgresses', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[UserVocabularyProgresses] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [UserId] nvarchar(64) NOT NULL,
                    [VocabularyId] uniqueidentifier NOT NULL,
                    [SenseId] uniqueidentifier NOT NULL,
                    [MasteryLevel] int NOT NULL DEFAULT 0,
                    [CorrectCount] int NOT NULL DEFAULT 0,
                    [IncorrectCount] int NOT NULL DEFAULT 0,
                    [ReviewCount] int NOT NULL DEFAULT 0,
                    [LastReviewedAt] datetimeoffset NULL,
                    [NextReviewAt] datetimeoffset NULL,
                    [IntervalDays] int NOT NULL DEFAULT 0,
                    [EaseFactor] decimal(5,2) NOT NULL DEFAULT 2.50,
                    [LearningStatus] nvarchar(32) NOT NULL DEFAULT 'New',
                    [LatestPronunciationScore] decimal(5,2) NULL
                );
                CREATE UNIQUE INDEX [IX_UserVocabularyProgresses_UserId_SenseId] ON [dbo].[UserVocabularyProgresses] ([UserId], [SenseId]);
                CREATE INDEX [IX_UserVocabularyProgresses_UserId_NextReviewAt] ON [dbo].[UserVocabularyProgresses] ([UserId], [NextReviewAt]);
            END

            IF OBJECT_ID('dbo.VocabularyReviewAttempts', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VocabularyReviewAttempts] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [UserId] nvarchar(64) NOT NULL,
                    [VocabularyId] uniqueidentifier NOT NULL,
                    [SenseId] uniqueidentifier NOT NULL,
                    [ReviewType] nvarchar(32) NOT NULL,
                    [Rating] nvarchar(16) NOT NULL,
                    [ReviewedAt] datetimeoffset NOT NULL,
                    [NextReviewAtSnapshot] datetimeoffset NULL
                );
            END

            IF OBJECT_ID('dbo.ScoringRubrics', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ScoringRubrics] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [Code] nvarchar(64) NOT NULL,
                    [Name] nvarchar(160) NOT NULL,
                    [Description] nvarchar(500) NOT NULL,
                    [Version] int NOT NULL DEFAULT 1,
                    [AssessmentType] nvarchar(32) NOT NULL DEFAULT 'Pronunciation',
                    [IsActive] bit NOT NULL DEFAULT 1,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NULL
                );
                CREATE UNIQUE INDEX [IX_ScoringRubrics_Code_Version] ON [dbo].[ScoringRubrics] ([Code], [Version]);
            END

            IF OBJECT_ID('dbo.ScoringRubricCriteria', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ScoringRubricCriteria] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [RubricId] uniqueidentifier NOT NULL,
                    [Code] nvarchar(64) NOT NULL,
                    [Name] nvarchar(128) NOT NULL,
                    [Description] nvarchar(500) NOT NULL,
                    [Weight] decimal(5,2) NOT NULL,
                    [MinimumScore] decimal(5,2) NOT NULL DEFAULT 0,
                    [MaximumScore] decimal(5,2) NOT NULL DEFAULT 100,
                    [DisplayOrder] int NOT NULL,
                    [IsEnabled] bit NOT NULL DEFAULT 1,
                    CONSTRAINT [FK_ScoringRubricCriteria_ScoringRubrics] FOREIGN KEY ([RubricId]) REFERENCES [dbo].[ScoringRubrics]([Id]) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('dbo.AudioRecordings', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[AudioRecordings] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [UserId] nvarchar(64) NOT NULL,
                    [StorageKey] nvarchar(256) NOT NULL,
                    [ContentType] nvarchar(64) NOT NULL,
                    [FileSizeBytes] bigint NOT NULL,
                    [DurationMilliseconds] int NOT NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [ExpiresAt] datetimeoffset NULL,
                    [DeletionStatus] nvarchar(32) NOT NULL DEFAULT 'Retained'
                );
            END

            IF OBJECT_ID('dbo.PronunciationAssessments', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[PronunciationAssessments] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [UserId] nvarchar(64) NOT NULL,
                    [VocabularyId] uniqueidentifier NOT NULL,
                    [SenseId] uniqueidentifier NOT NULL,
                    [TargetType] nvarchar(32) NOT NULL,
                    [TargetId] uniqueidentifier NULL,
                    [ReferenceText] nvarchar(1000) NOT NULL,
                    [Accent] nvarchar(16) NOT NULL,
                    [ModelCode] nvarchar(128) NOT NULL,
                    [RubricId] uniqueidentifier NOT NULL,
                    [RubricVersion] int NOT NULL,
                    [AudioRecordingId] uniqueidentifier NOT NULL,
                    [RecognizedText] nvarchar(1000) NULL,
                    [OverallScore] decimal(5,2) NULL,
                    [Confidence] decimal(5,2) NULL,
                    [Status] nvarchar(32) NOT NULL,
                    [Feedback] nvarchar(max) NULL,
                    [StrengthsJson] nvarchar(max) NULL,
                    [ImprovementsJson] nvarchar(max) NULL,
                    [SuggestedPracticeJson] nvarchar(max) NULL,
                    [ErrorCode] nvarchar(64) NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [CompletedAt] datetimeoffset NULL
                );
                CREATE INDEX [IX_PronunciationAssessments_UserId_VocabularyId_CreatedAt] ON [dbo].[PronunciationAssessments] ([UserId], [VocabularyId], [CreatedAt]);
            END

            IF OBJECT_ID('dbo.PronunciationCriterionScores', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[PronunciationCriterionScores] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [AssessmentId] uniqueidentifier NOT NULL,
                    [CriterionCode] nvarchar(64) NOT NULL,
                    [CriterionName] nvarchar(128) NOT NULL,
                    [CriterionDescriptionSnapshot] nvarchar(500) NOT NULL,
                    [WeightSnapshot] decimal(5,2) NOT NULL,
                    [RawScore] decimal(5,2) NOT NULL,
                    [WeightedScore] decimal(5,2) NOT NULL,
                    [Feedback] nvarchar(500) NOT NULL,
                    CONSTRAINT [FK_PronunciationCriterionScores_PronunciationAssessments] FOREIGN KEY ([AssessmentId]) REFERENCES [dbo].[PronunciationAssessments]([Id]) ON DELETE CASCADE
                );
            END
            """);

        // 2. Seed AI Model Catalog idempotently (Section 5.1 & 5.2)
        if (!await db.AiModelCatalog.AnyAsync())
        {
            var models = new List<AiModelConfigEntity>
            {
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    DisplayName = "Gemini 3.8 Flash (Recommended)",
                    ModelCode = "gemini-3.8-flash",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Model đa phương thức thế hệ mới nhất, hỗ trợ cả tạo từ vựng JSON và chấm điểm phát âm trực tiếp từ âm thanh.",
                    DisplayOrder = 1
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    DisplayName = "Gemini 3.6 Flash",
                    ModelCode = "gemini-3.6-flash",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Hỗ trợ tạo từ vựng và đánh giá phát âm bằng âm thanh.",
                    DisplayOrder = 2
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                    DisplayName = "Gemini 3.5 Flash Lite",
                    ModelCode = "gemini-3.5-flash-lite",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Tốc độ phản hồi cực nhanh, chi phí thấp.",
                    DisplayOrder = 3
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                    DisplayName = "Gemini 3.1 Flash Lite",
                    ModelCode = "gemini-3.1-flash-lite",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Phiên bản nhẹ hỗ trợ cả tạo từ vựng và chấm phát âm.",
                    DisplayOrder = 4
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                    DisplayName = "Gemini 3 Flash Preview",
                    ModelCode = "gemini-3-flash-preview",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Bản preview dòng Gemini 3 Flash.",
                    DisplayOrder = 5
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                    DisplayName = "Gemini 2.5 Flash",
                    ModelCode = "gemini-2.5-flash",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Model ổn định phổ biến; lưu ý một số tài khoản mới có thể giới hạn quyền truy cập dòng 2.5.",
                    DisplayOrder = 6
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                    DisplayName = "Gemini 2.5 Flash Lite",
                    ModelCode = "gemini-2.5-flash-lite",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Phiên bản Lite của dòng Gemini 2.5.",
                    DisplayOrder = 7
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000008"),
                    DisplayName = "Gemini 3.7 Flash (Deprecated → 3.8 Flash)",
                    ModelCode = "gemini-3.7-flash",
                    RoutedModelCode = "gemini-3.8-flash",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = true,
                    SupportsPronunciationAssessment = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = true,
                    IsEnabled = true,
                    CapabilityNote = "Đã ngừng khuyến nghị (Deprecated) và tự động định tuyến sang gemini-3.8-flash.",
                    DisplayOrder = 8
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000009"),
                    DisplayName = "Gemma 4 26B (Text Only)",
                    ModelCode = "gemma-4-26b-a4b-it",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = false,
                    SupportsPronunciationAssessment = false,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Chỉ hỗ trợ tạo từ vựng văn bản (Text Generation); KHÔNG hỗ trợ đầu vào âm thanh để chấm phát âm.",
                    DisplayOrder = 9
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000010"),
                    DisplayName = "Gemma 4 31B (Text Only)",
                    ModelCode = "gemma-4-31b-it",
                    SupportsTextGeneration = true,
                    SupportsStructuredOutput = true,
                    SupportsAudioInput = false,
                    SupportsPronunciationAssessment = false,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsDeprecated = false,
                    IsEnabled = true,
                    CapabilityNote = "Chỉ hỗ trợ tạo từ vựng văn bản (Text Generation); KHÔNG hỗ trợ đầu vào âm thanh để chấm phát âm.",
                    DisplayOrder = 10
                },
                // Specialized Realtime Models (kept separate from standard selectors)
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000011"),
                    DisplayName = "Gemini 3.5 Transcribe Live",
                    ModelCode = "gemini-3.5-transcribe-live",
                    SupportsLiveAudio = true,
                    SupportsTranscription = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsEnabled = true,
                    CapabilityNote = "Chuyên dụng cho Hub 2 Realtime Transcription.",
                    DisplayOrder = 11
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000012"),
                    DisplayName = "Gemini 3.5 Live Translate Preview",
                    ModelCode = "gemini-3.5-live-translate-preview",
                    SupportsLiveAudio = true,
                    SupportsTranslation = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsEnabled = true,
                    CapabilityNote = "Chuyên dụng cho Hub 3 Realtime Vietnamese Translation.",
                    DisplayOrder = 12
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000013"),
                    DisplayName = "Gemini 3.8 Live",
                    ModelCode = "gemini-3.8-live",
                    SupportsLiveAudio = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsEnabled = true,
                    CapabilityNote = "Chuyên dụng cho Hub 1 Live Voice Tutor.",
                    DisplayOrder = 13
                },
                new()
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000014"),
                    DisplayName = "Gemini 3.8 Live Extended Thinking",
                    ModelCode = "gemini-3.8-live-extended-thinking",
                    SupportsLiveAudio = true,
                    SupportsStreaming = true,
                    IsAvailable = true,
                    IsEnabled = true,
                    CapabilityNote = "Chuyên dụng cho Hub 1 Live Voice Tutor (Thinking Mode).",
                    DisplayOrder = 14
                }
            };

            db.AiModelCatalog.AddRange(models);
            await db.SaveChangesAsync();
        }

        // 3. Seed Default Pronunciation Scoring Rubric ENGLISH_PRONUNCIATION_V1 (Section 9.3)
        if (!await db.ScoringRubrics.AnyAsync(r => r.Code == "ENGLISH_PRONUNCIATION_V1"))
        {
            var rubricId = Guid.Parse("20000000-0000-0000-0000-000000000001");
            var rubric = new ScoringRubricEntity
            {
                Id = rubricId,
                Code = "ENGLISH_PRONUNCIATION_V1",
                Name = "Tiêu chuẩn chấm phát âm tiếng Anh v1 (Accuracy 40% · Clarity 30% · Fluency 30%)",
                Description = "Đánh giá phát âm từ vựng và câu ví dụ dựa trên độ chính xác âm vị, độ rõ ràng và độ trôi chảy.",
                Version = 1,
                AssessmentType = "Pronunciation",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                Criteria = new List<ScoringRubricCriterionEntity>
                {
                    new()
                    {
                        Id = Guid.Parse("20000000-0000-0000-0000-000000000101"),
                        RubricId = rubricId,
                        Code = "Accuracy",
                        Name = "Accuracy (Độ chính xác âm vị & trọng âm)",
                        Description = "Phonetic accuracy of vowel and consonant sounds and stress patterns.",
                        Weight = 40.00m,
                        MinimumScore = 0m,
                        MaximumScore = 100m,
                        DisplayOrder = 1,
                        IsEnabled = true
                    },
                    new()
                    {
                        Id = Guid.Parse("20000000-0000-0000-0000-000000000102"),
                        RubricId = rubricId,
                        Code = "Clarity",
                        Name = "Clarity (Độ rõ chữ & khẩu hình)",
                        Description = "Vocal projection, articulation clarity, and audibility consistency.",
                        Weight = 30.00m,
                        MinimumScore = 0m,
                        MaximumScore = 100m,
                        DisplayOrder = 2,
                        IsEnabled = true
                    },
                    new()
                    {
                        Id = Guid.Parse("20000000-0000-0000-0000-000000000103"),
                        RubricId = rubricId,
                        Code = "Fluency",
                        Name = "Fluency (Nhịp điệu & độ trôi chảy)",
                        Description = "Speaking rhythm, natural flow, and absence of hesitation pauses.",
                        Weight = 30.00m,
                        MinimumScore = 0m,
                        MaximumScore = 100m,
                        DisplayOrder = 3,
                        IsEnabled = true
                    }
                }
            };
            db.ScoringRubrics.Add(rubric);
            await db.SaveChangesAsync();
        }

        // 4. Seed Starter Vocabulary Catalog (A1, A2, B1, B2) idempotently (Section 18)
        if (!await db.Vocabularies.AnyAsync())
        {
            var starterVocabs = BuildStarterVocabularyCatalog();
            db.Vocabularies.AddRange(starterVocabs);
            await db.SaveChangesAsync();
        }
    }

    private static List<VocabularyEntity> BuildStarterVocabularyCatalog()
    {
        var list = new List<VocabularyEntity>();

        // Word 1: beautiful (A1)
        var vBeautifulId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var sBeautifulId = Guid.Parse("30000000-0000-0000-0000-000000000101");
        list.Add(new VocabularyEntity
        {
            Id = vBeautifulId,
            Word = "beautiful",
            NormalizedWord = "beautiful",
            LanguageCode = "en",
            FrequencyRank = 320,
            SourceType = "Seeded",
            TopicCategory = "Daily Life & Descriptions",
            Pronunciations = new List<VocabularyPronunciationEntity>
            {
                new() { Accent = "UK", Ipa = "/ˈbjuː.tɪ.fəl/", SyllableCount = 3, StressPattern = "BEAU-ti-ful (Ooo)" },
                new() { Accent = "US", Ipa = "/ˈbjuː.t̬ə.fəl/", SyllableCount = 3, StressPattern = "BEAU-ti-ful (Ooo)" }
            },
            WordFamilies = new List<VocabularyWordFamilyEntity>
            {
                new() { RelatedWord = "beauty", PartOfSpeech = "noun" },
                new() { RelatedWord = "beautify", PartOfSpeech = "verb" },
                new() { RelatedWord = "beautifully", PartOfSpeech = "adverb" }
            },
            Senses = new List<VocabularySenseEntity>
            {
                new()
                {
                    Id = sBeautifulId,
                    VocabularyId = vBeautifulId,
                    PartOfSpeech = "adjective",
                    CefrLevel = "A1",
                    DefinitionEn = "Pleasing the senses or mind aesthetically; very attractive.",
                    MeaningVi = "Đẹp, xinh đẹp, tuyệt đẹp",
                    DescriptionEn = "Used to describe people, places, scenery, weather, or experiences that give great pleasure to see or hear.",
                    IsPrimary = true,
                    DisplayOrder = 1,
                    Relations = new List<VocabularyRelationEntity>
                    {
                        new() { RelationType = "Synonym", RelatedWord = "gorgeous", DisplayOrder = 1 },
                        new() { RelationType = "Synonym", RelatedWord = "stunning", DisplayOrder = 2 },
                        new() { RelationType = "Synonym", RelatedWord = "attractive", DisplayOrder = 3 },
                        new() { RelationType = "Antonym", RelatedWord = "ugly", DisplayOrder = 1 },
                        new() { RelationType = "Antonym", RelatedWord = "unattractive", DisplayOrder = 2 }
                    },
                    Collocations = new List<VocabularyCollocationEntity>
                    {
                        new() { PhraseEn = "beautiful scenery", MeaningVi = "phong cảnh tuyệt đẹp", DisplayOrder = 1 },
                        new() { PhraseEn = "beautiful smile", MeaningVi = "nụ cười rạng rỡ", DisplayOrder = 2 },
                        new() { PhraseEn = "absolutely beautiful", MeaningVi = "đẹp hoàn hảo / cực kỳ đẹp", DisplayOrder = 3 }
                    },
                    Examples = new List<VocabularyExampleEntity>
                    {
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001001"),
                            SentenceEn = "Da Nang is famous for its beautiful beaches and friendly people.",
                            SentenceVi = "Đà Nẵng nổi tiếng với những bãi biển tuyệt đẹp và con người thân thiện.",
                            CefrLevel = "A1",
                            DisplayOrder = 1
                        },
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001002"),
                            SentenceEn = "She played a beautiful melody on the piano.",
                            SentenceVi = "Cô ấy đã chơi một giai điệu tuyệt đẹp trên đàn dương cầm.",
                            CefrLevel = "A2",
                            DisplayOrder = 2
                        }
                    },
                    GrammarPatterns = new List<VocabularyGrammarPatternEntity>
                    {
                        new() { Pattern = "beautiful + noun", Explanation = "Đứng trước danh từ để bổ nghĩa", Example = "a beautiful morning" },
                        new() { Pattern = "be / look / sound + beautiful", Explanation = "Đứng sau động từ nối (linking verbs)", Example = "The sunset looks beautiful." }
                    }
                }
            }
        });

        // Word 2: bank (Multi-sense example required by Section 4.2: Sense 1 A2 financial, Sense 2 B1 river bank)
        var vBankId = Guid.Parse("30000000-0000-0000-0000-000000000002");
        list.Add(new VocabularyEntity
        {
            Id = vBankId,
            Word = "bank",
            NormalizedWord = "bank",
            LanguageCode = "en",
            FrequencyRank = 410,
            SourceType = "Seeded",
            TopicCategory = "Finance & Geography (Multi-Sense)",
            Pronunciations = new List<VocabularyPronunciationEntity>
            {
                new() { Accent = "UK", Ipa = "/bæŋk/", SyllableCount = 1, StressPattern = "bank (O)" },
                new() { Accent = "US", Ipa = "/bæŋk/", SyllableCount = 1, StressPattern = "bank (O)" }
            },
            WordFamilies = new List<VocabularyWordFamilyEntity>
            {
                new() { RelatedWord = "banker", PartOfSpeech = "noun" },
                new() { RelatedWord = "banking", PartOfSpeech = "noun" }
            },
            Senses = new List<VocabularySenseEntity>
            {
                new()
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000102"),
                    VocabularyId = vBankId,
                    PartOfSpeech = "noun",
                    CefrLevel = "A2",
                    DefinitionEn = "An organization where people and businesses can invest or borrow money and exchange currency.",
                    MeaningVi = "Ngân hàng (tổ chức tài chính)",
                    DescriptionEn = "Sense 1: Financial institution where you keep money in an account or apply for loans.",
                    IsPrimary = true,
                    DisplayOrder = 1,
                    Relations = new List<VocabularyRelationEntity>
                    {
                        new() { RelationType = "Synonym", RelatedWord = "financial institution", DisplayOrder = 1 },
                        new() { RelationType = "Synonym", RelatedWord = "lender", DisplayOrder = 2 }
                    },
                    Collocations = new List<VocabularyCollocationEntity>
                    {
                        new() { PhraseEn = "open a bank account", MeaningVi = "mở tài khoản ngân hàng", DisplayOrder = 1 },
                        new() { PhraseEn = "bank transfer", MeaningVi = "chuyển khoản ngân hàng", DisplayOrder = 2 }
                    },
                    Examples = new List<VocabularyExampleEntity>
                    {
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001003"),
                            SentenceEn = "I need to go to the bank to open a savings account.",
                            SentenceVi = "Tôi cần đến ngân hàng để mở một tài khoản tiết kiệm.",
                            CefrLevel = "A2",
                            DisplayOrder = 1
                        }
                    },
                    GrammarPatterns = new List<VocabularyGrammarPatternEntity>
                    {
                        new() { Pattern = "deposit / withdraw money from a bank", Explanation = "Gửi / rút tiền từ ngân hàng", Example = "She withdrew cash from the bank." }
                    }
                },
                new()
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000103"),
                    VocabularyId = vBankId,
                    PartOfSpeech = "noun",
                    CefrLevel = "B1",
                    DefinitionEn = "Sloping raised land, especially along the sides of a river or lake.",
                    MeaningVi = "Bờ sông, bờ hồ",
                    DescriptionEn = "Sense 2: The land alongside a body of water such as a river or canal.",
                    IsPrimary = false,
                    DisplayOrder = 2,
                    Relations = new List<VocabularyRelationEntity>
                    {
                        new() { RelationType = "Synonym", RelatedWord = "riverbank", DisplayOrder = 1 },
                        new() { RelationType = "Synonym", RelatedWord = "shore", DisplayOrder = 2 }
                    },
                    Collocations = new List<VocabularyCollocationEntity>
                    {
                        new() { PhraseEn = "along the river bank", MeaningVi = "dọc theo bờ sông", DisplayOrder = 1 },
                        new() { PhraseEn = "steep grassy bank", MeaningVi = "bờ cỏ dốc", DisplayOrder = 2 }
                    },
                    Examples = new List<VocabularyExampleEntity>
                    {
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001004"),
                            SentenceEn = "We walked peacefully along the grassy bank of the Red River at sunset.",
                            SentenceVi = "Chúng tôi đi dạo yên bình dọc theo bờ cỏ của sông Hồng lúc hoàng hôn.",
                            CefrLevel = "B1",
                            DisplayOrder = 1
                        }
                    },
                    GrammarPatterns = new List<VocabularyGrammarPatternEntity>
                    {
                        new() { Pattern = "on the bank of + the river", Explanation = "Trên bờ của con sông", Example = "A small village sits on the bank of the river." }
                    }
                }
            }
        });

        // Word 3: collaborate (B1)
        var vCollabId = Guid.Parse("30000000-0000-0000-0000-000000000003");
        list.Add(new VocabularyEntity
        {
            Id = vCollabId,
            Word = "collaborate",
            NormalizedWord = "collaborate",
            LanguageCode = "en",
            FrequencyRank = 1450,
            SourceType = "Seeded",
            TopicCategory = "Workplace & Teamwork",
            Pronunciations = new List<VocabularyPronunciationEntity>
            {
                new() { Accent = "UK", Ipa = "/kəˈlæb.ə.reɪt/", SyllableCount = 4, StressPattern = "col-LAB-o-rate (oOoo)" },
                new() { Accent = "US", Ipa = "/kəˈlæb.ə.reɪt/", SyllableCount = 4, StressPattern = "col-LAB-o-rate (oOoo)" }
            },
            WordFamilies = new List<VocabularyWordFamilyEntity>
            {
                new() { RelatedWord = "collaboration", PartOfSpeech = "noun" },
                new() { RelatedWord = "collaborative", PartOfSpeech = "adjective" },
                new() { RelatedWord = "collaborator", PartOfSpeech = "noun" }
            },
            Senses = new List<VocabularySenseEntity>
            {
                new()
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000104"),
                    VocabularyId = vCollabId,
                    PartOfSpeech = "verb",
                    CefrLevel = "B1",
                    DefinitionEn = "To work together with one or more people or organizations to achieve a shared goal.",
                    MeaningVi = "Hợp tác, cộng tác làm việc cùng nhau",
                    DescriptionEn = "Essential verb for describing cross-functional teamwork in interviews and daily standups.",
                    IsPrimary = true,
                    DisplayOrder = 1,
                    Relations = new List<VocabularyRelationEntity>
                    {
                        new() { RelationType = "Synonym", RelatedWord = "cooperate", DisplayOrder = 1 },
                        new() { RelationType = "Synonym", RelatedWord = "team up", DisplayOrder = 2 },
                        new() { RelationType = "Antonym", RelatedWord = "work alone", DisplayOrder = 1 }
                    },
                    Collocations = new List<VocabularyCollocationEntity>
                    {
                        new() { PhraseEn = "collaborate closely with", MeaningVi = "hợp tác chặt chẽ với", DisplayOrder = 1 },
                        new() { PhraseEn = "collaborate on a project", MeaningVi = "cộng tác trong một dự án", DisplayOrder = 2 }
                    },
                    Examples = new List<VocabularyExampleEntity>
                    {
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001005"),
                            SentenceEn = "Our backend engineers collaborate closely with the frontend team every sprint.",
                            SentenceVi = "Các kỹ sư backend của chúng tôi hợp tác chặt chẽ với đội frontend trong mỗi sprint.",
                            CefrLevel = "B1",
                            DisplayOrder = 1
                        }
                    },
                    GrammarPatterns = new List<VocabularyGrammarPatternEntity>
                    {
                        new() { Pattern = "collaborate with [somebody] on [something]", Explanation = "Hợp tác với ai về việc gì", Example = "We collaborated with the QA team on automated testing." }
                    }
                }
            }
        });

        // Word 4: scalable (B2)
        var vScalableId = Guid.Parse("30000000-0000-0000-0000-000000000004");
        list.Add(new VocabularyEntity
        {
            Id = vScalableId,
            Word = "scalable",
            NormalizedWord = "scalable",
            LanguageCode = "en",
            FrequencyRank = 2800,
            SourceType = "Seeded",
            TopicCategory = "Software Engineering & System Design",
            Pronunciations = new List<VocabularyPronunciationEntity>
            {
                new() { Accent = "UK", Ipa = "/ˈskeɪ.lə.bəl/", SyllableCount = 3, StressPattern = "SCAL-a-ble (Ooo)" },
                new() { Accent = "US", Ipa = "/ˈskeɪ.lə.bəl/", SyllableCount = 3, StressPattern = "SCAL-a-ble (Ooo)" }
            },
            WordFamilies = new List<VocabularyWordFamilyEntity>
            {
                new() { RelatedWord = "scalability", PartOfSpeech = "noun" },
                new() { RelatedWord = "scale", PartOfSpeech = "verb" }
            },
            Senses = new List<VocabularySenseEntity>
            {
                new()
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000105"),
                    VocabularyId = vScalableId,
                    PartOfSpeech = "adjective",
                    CefrLevel = "B2",
                    DefinitionEn = "Able to grow, expand, or handle a rapidly increasing amount of work or traffic smoothly.",
                    MeaningVi = "Có khả năng mở rộng quy mô / chịu tải cao",
                    DescriptionEn = "Frequently used in System Design and Technical Interviews to describe cloud architectures and databases.",
                    IsPrimary = true,
                    DisplayOrder = 1,
                    Relations = new List<VocabularyRelationEntity>
                    {
                        new() { RelationType = "Synonym", RelatedWord = "expandable", DisplayOrder = 1 },
                        new() { RelationType = "Synonym", RelatedWord = "extensible", DisplayOrder = 2 },
                        new() { RelationType = "Antonym", RelatedWord = "rigid", DisplayOrder = 1 }
                    },
                    Collocations = new List<VocabularyCollocationEntity>
                    {
                        new() { PhraseEn = "highly scalable architecture", MeaningVi = "kiến trúc có khả năng mở rộng cao", DisplayOrder = 1 },
                        new() { PhraseEn = "scalable cloud solution", MeaningVi = "giải pháp đám mây dễ mở rộng", DisplayOrder = 2 }
                    },
                    Examples = new List<VocabularyExampleEntity>
                    {
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001006"),
                            SentenceEn = "We designed a highly scalable microservices architecture to handle ten thousand concurrent users.",
                            SentenceVi = "Chúng tôi đã thiết kế một kiến trúc microservices có khả năng mở rộng cao để xử lý mười nghìn người dùng đồng thời.",
                            CefrLevel = "B2",
                            DisplayOrder = 1
                        }
                    },
                    GrammarPatterns = new List<VocabularyGrammarPatternEntity>
                    {
                        new() { Pattern = "highly / easily + scalable", Explanation = "Đi kèm trạng từ chỉ mức độ", Example = "The database cluster is easily scalable." }
                    }
                }
            }
        });

        // Word 5: itinerary (B1)
        var vItineraryId = Guid.Parse("30000000-0000-0000-0000-000000000005");
        list.Add(new VocabularyEntity
        {
            Id = vItineraryId,
            Word = "itinerary",
            NormalizedWord = "itinerary",
            LanguageCode = "en",
            FrequencyRank = 3400,
            SourceType = "Seeded",
            TopicCategory = "Travel & Planning",
            Pronunciations = new List<VocabularyPronunciationEntity>
            {
                new() { Accent = "UK", Ipa = "/aɪˈtɪn.ər.ər.i/", SyllableCount = 5, StressPattern = "i-TIN-er-ar-y (oOooo)" },
                new() { Accent = "US", Ipa = "/aɪˈtɪn.ə.rer.i/", SyllableCount = 5, StressPattern = "i-TIN-er-ar-y (oOooo)" }
            },
            WordFamilies = new List<VocabularyWordFamilyEntity>
            {
                new() { RelatedWord = "itinerant", PartOfSpeech = "adjective" }
            },
            Senses = new List<VocabularySenseEntity>
            {
                new()
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000106"),
                    VocabularyId = vItineraryId,
                    PartOfSpeech = "noun",
                    CefrLevel = "B1",
                    DefinitionEn = "A detailed plan or route of a journey, including places to visit and times.",
                    MeaningVi = "Lịch trình chuyến đi, hành trình du lịch",
                    DescriptionEn = "Commonly used when discussing travel plans, business trips, and tour schedules.",
                    IsPrimary = true,
                    DisplayOrder = 1,
                    Relations = new List<VocabularyRelationEntity>
                    {
                        new() { RelationType = "Synonym", RelatedWord = "travel schedule", DisplayOrder = 1 },
                        new() { RelationType = "Synonym", RelatedWord = "route plan", DisplayOrder = 2 }
                    },
                    Collocations = new List<VocabularyCollocationEntity>
                    {
                        new() { PhraseEn = "plan a travel itinerary", MeaningVi = "lên lịch trình chuyến đi", DisplayOrder = 1 },
                        new() { PhraseEn = "packed itinerary", MeaningVi = "lịch trình dày đặc", DisplayOrder = 2 }
                    },
                    Examples = new List<VocabularyExampleEntity>
                    {
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001007"),
                            SentenceEn = "Could you send me the detailed itinerary for our business trip to Singapore?",
                            SentenceVi = "Bạn có thể gửi cho tôi lịch trình chi tiết cho chuyến công tác tới Singapore của chúng ta không?",
                            CefrLevel = "B1",
                            DisplayOrder = 1
                        }
                    },
                    GrammarPatterns = new List<VocabularyGrammarPatternEntity>
                    {
                        new() { Pattern = "on the itinerary", Explanation = "Có trong lịch trình", Example = "Visiting the old quarter is next on our itinerary." }
                    }
                }
            }
        });

        // Word 6: resilient (B2)
        var vResilientId = Guid.Parse("30000000-0000-0000-0000-000000000006");
        list.Add(new VocabularyEntity
        {
            Id = vResilientId,
            Word = "resilient",
            NormalizedWord = "resilient",
            LanguageCode = "en",
            FrequencyRank = 2650,
            SourceType = "Seeded",
            TopicCategory = "Personal Growth & System Engineering",
            Pronunciations = new List<VocabularyPronunciationEntity>
            {
                new() { Accent = "UK", Ipa = "/rɪˈzɪl.i.ənt/", SyllableCount = 4, StressPattern = "re-SIL-i-ent (oOoo)" },
                new() { Accent = "US", Ipa = "/rɪˈzɪl.jənt/", SyllableCount = 3, StressPattern = "re-SIL-ient (oOo)" }
            },
            WordFamilies = new List<VocabularyWordFamilyEntity>
            {
                new() { RelatedWord = "resilience", PartOfSpeech = "noun" },
                new() { RelatedWord = "resiliently", PartOfSpeech = "adverb" }
            },
            Senses = new List<VocabularySenseEntity>
            {
                new()
                {
                    Id = Guid.Parse("30000000-0000-0000-0000-000000000107"),
                    VocabularyId = vResilientId,
                    PartOfSpeech = "adjective",
                    CefrLevel = "B2",
                    DefinitionEn = "Able to recover quickly from difficult conditions, setbacks, or system failures.",
                    MeaningVi = "Kiên cường, bền bỉ; có khả năng tự phục hồi lỗi",
                    DescriptionEn = "Great word for both HR behavioral interviews (personal resilience) and System Design interviews (fault-tolerant systems).",
                    IsPrimary = true,
                    DisplayOrder = 1,
                    Relations = new List<VocabularyRelationEntity>
                    {
                        new() { RelationType = "Synonym", RelatedWord = "adaptable", DisplayOrder = 1 },
                        new() { RelationType = "Synonym", RelatedWord = "fault-tolerant", DisplayOrder = 2 },
                        new() { RelationType = "Antonym", RelatedWord = "fragile", DisplayOrder = 1 }
                    },
                    Collocations = new List<VocabularyCollocationEntity>
                    {
                        new() { PhraseEn = "resilient to failure", MeaningVi = "có khả năng chống chịu lỗi", DisplayOrder = 1 },
                        new() { PhraseEn = "stay resilient under pressure", MeaningVi = "giữ vững sự kiên cường dưới áp lực", DisplayOrder = 2 }
                    },
                    Examples = new List<VocabularyExampleEntity>
                    {
                        new()
                        {
                            Id = Guid.Parse("30000000-0000-0000-0000-000000001008"),
                            SentenceEn = "Adding automatic retry policies made our payment service much more resilient to network timeouts.",
                            SentenceVi = "Việc bổ sung cơ chế tự động thử lại đã giúp dịch vụ thanh toán của chúng tôi chịu lỗi mất mạng tốt hơn nhiều.",
                            CefrLevel = "B2",
                            DisplayOrder = 1
                        }
                    },
                    GrammarPatterns = new List<VocabularyGrammarPatternEntity>
                    {
                        new() { Pattern = "be resilient to + noun", Explanation = "Chống chịu tốt trước điều gì", Example = "The system is resilient to sudden traffic spikes." }
                    }
                }
            }
        });

        return list;
    }
}
