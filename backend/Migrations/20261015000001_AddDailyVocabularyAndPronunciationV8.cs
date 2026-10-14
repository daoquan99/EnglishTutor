using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLiveTutor.Api.Migrations;

public partial class AddDailyVocabularyAndPronunciationV8 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent creation of all v8 Daily Vocabulary, AI Model Catalog, and Pronunciation tables
        // is also executed automatically on startup by DatabaseSeeder.EnsureSchemaAndSeedAsync(dbFactory).
        migrationBuilder.Sql(@"
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
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS [dbo].[PronunciationCriterionScores];
            DROP TABLE IF EXISTS [dbo].[PronunciationAssessments];
            DROP TABLE IF EXISTS [dbo].[AudioRecordings];
            DROP TABLE IF EXISTS [dbo].[ScoringRubricCriteria];
            DROP TABLE IF EXISTS [dbo].[ScoringRubrics];
            DROP TABLE IF EXISTS [dbo].[VocabularyGenerationAudits];
            DROP TABLE IF EXISTS [dbo].[VocabularyReviewAttempts];
            DROP TABLE IF EXISTS [dbo].[UserVocabularyProgresses];
            DROP TABLE IF EXISTS [dbo].[DailyVocabularyItems];
            DROP TABLE IF EXISTS [dbo].[DailyVocabularyPlans];
            DROP TABLE IF EXISTS [dbo].[AiModelCatalog];
            DROP TABLE IF EXISTS [dbo].[VocabularyGrammarPatterns];
            DROP TABLE IF EXISTS [dbo].[VocabularyExamples];
            DROP TABLE IF EXISTS [dbo].[VocabularyCollocations];
            DROP TABLE IF EXISTS [dbo].[VocabularyWordFamilies];
            DROP TABLE IF EXISTS [dbo].[VocabularyRelations];
            DROP TABLE IF EXISTS [dbo].[VocabularyPronunciations];
            DROP TABLE IF EXISTS [dbo].[VocabularySenses];
            DROP TABLE IF EXISTS [dbo].[Vocabularies];
        ");
    }
}
