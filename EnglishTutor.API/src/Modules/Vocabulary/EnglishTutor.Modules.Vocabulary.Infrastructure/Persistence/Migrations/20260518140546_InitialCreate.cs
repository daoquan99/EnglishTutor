using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vocabulary");

            migrationBuilder.CreateTable(
                name: "ExampleSentencePronunciationAttempts",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyExampleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RecognizedText = table.Column<string>(type: "text", nullable: false),
                    PronunciationScore = table.Column<int>(type: "integer", nullable: false),
                    AccuracyScore = table.Column<int>(type: "integer", nullable: false),
                    FluencyScore = table.Column<int>(type: "integer", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    AttemptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleSentencePronunciationAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedBy = table.Column<string>(type: "text", nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserVocabularyMasteries",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MeaningMasteryScore = table.Column<int>(type: "integer", nullable: false),
                    PronunciationMasteryScore = table.Column<int>(type: "integer", nullable: false),
                    ExampleSentenceScore = table.Column<int>(type: "integer", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    CorrectReviewCount = table.Column<int>(type: "integer", nullable: false),
                    LastReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReviewAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVocabularyMasteries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyItems",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Word = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phonetic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Level = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Topic = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyPronunciationAttempts",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: true),
                    RecognizedText = table.Column<string>(type: "text", nullable: false),
                    PronunciationScore = table.Column<int>(type: "integer", nullable: false),
                    AccuracyScore = table.Column<int>(type: "integer", nullable: false),
                    FluencyScore = table.Column<int>(type: "integer", nullable: false),
                    CompletenessScore = table.Column<int>(type: "integer", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    AttemptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyPronunciationAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyReviewAttempts",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyReviewAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyReviewSessions",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyReviewSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyExamples",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Sentence = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyExamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyExamples_VocabularyItems_VocabularyItemId",
                        column: x => x.VocabularyItemId,
                        principalSchema: "vocabulary",
                        principalTable: "VocabularyItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyTranslations",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Meaning = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyTranslations_VocabularyItems_VocabularyItemId",
                        column: x => x.VocabularyItemId,
                        principalSchema: "vocabulary",
                        principalTable: "VocabularyItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyExampleTranslations",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyExampleId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Translation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyExampleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyExampleTranslations_VocabularyExamples_Vocabulary~",
                        column: x => x.VocabularyExampleId,
                        principalSchema: "vocabulary",
                        principalTable: "VocabularyExamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextRetryAtUtc",
                schema: "vocabulary",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextRetryAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyMasteries_UserId_NextReviewAtUtc",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                columns: new[] { "UserId", "NextReviewAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyMasteries_UserId_VocabularyItemId_TargetLangu~",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                columns: new[] { "UserId", "VocabularyItemId", "TargetLanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyExamples_VocabularyItemId",
                schema: "vocabulary",
                table: "VocabularyExamples",
                column: "VocabularyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyExampleTranslations_VocabularyExampleId",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations",
                column: "VocabularyExampleId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyItems_TargetLanguageCode_Word_PartOfSpeech",
                schema: "vocabulary",
                table: "VocabularyItems",
                columns: new[] { "TargetLanguageCode", "Word", "PartOfSpeech" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyPronunciationAttempts_UserId_VocabularyItemId",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                columns: new[] { "UserId", "VocabularyItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTranslations_VocabularyItemId",
                schema: "vocabulary",
                table: "VocabularyTranslations",
                column: "VocabularyItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExampleSentencePronunciationAttempts",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "UserVocabularyMasteries",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyExampleTranslations",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyPronunciationAttempts",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyReviewAttempts",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyReviewSessions",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyTranslations",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyExamples",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyItems",
                schema: "vocabulary");
        }
    }
}
