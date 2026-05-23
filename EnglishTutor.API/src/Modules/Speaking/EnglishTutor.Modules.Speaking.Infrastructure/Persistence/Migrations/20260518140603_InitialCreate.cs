using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "speaking");

            migrationBuilder.CreateTable(
                name: "ConversationPracticeResults",
                schema: "speaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeakingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskCompletionScore = table.Column<int>(type: "integer", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationPracticeResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "speaking",
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
                name: "SpeakingSessions",
                schema: "speaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NativeLanguageCodeAtStart = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TargetLanguageCodeAtStart = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    UiLanguageCodeAtStart = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExplanationLanguageCodeAtStart = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    UserLevelAtStart = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpeakingSessionSummaries",
                schema: "speaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeakingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AverageGrammarScore = table.Column<int>(type: "integer", nullable: false),
                    AverageVocabularyScore = table.Column<int>(type: "integer", nullable: false),
                    AveragePronunciationScore = table.Column<int>(type: "integer", nullable: false),
                    AverageFluencyScore = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    TotalTurns = table.Column<int>(type: "integer", nullable: false),
                    TotalMistakes = table.Column<int>(type: "integer", nullable: false),
                    StrongPoints = table.Column<string>(type: "text", nullable: false),
                    WeakPoints = table.Column<string>(type: "text", nullable: false),
                    Recommendation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakingSessionSummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpeakingTurnResults",
                schema: "speaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeakingTurnId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OriginalText = table.Column<string>(type: "text", nullable: false),
                    CorrectedText = table.Column<string>(type: "text", nullable: false),
                    NaturalVersion = table.Column<string>(type: "text", nullable: false),
                    GrammarScore = table.Column<int>(type: "integer", nullable: false),
                    VocabularyScore = table.Column<int>(type: "integer", nullable: false),
                    PronunciationScore = table.Column<int>(type: "integer", nullable: false),
                    FluencyScore = table.Column<int>(type: "integer", nullable: false),
                    TaskCompletionScore = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    FeedbackLanguageCode = table.Column<string>(type: "text", nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: true),
                    RecognizedText = table.Column<string>(type: "text", nullable: true),
                    WordLevelFeedbackJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakingTurnResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpeakingTurns",
                schema: "speaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeakingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    UserText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AudioUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakingTurns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeakingTurns_SpeakingSessions_SpeakingSessionId",
                        column: x => x.SpeakingSessionId,
                        principalSchema: "speaking",
                        principalTable: "SpeakingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextRetryAtUtc",
                schema: "speaking",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextRetryAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingSessionSummaries_SpeakingSessionId",
                schema: "speaking",
                table: "SpeakingSessionSummaries",
                column: "SpeakingSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingTurnResults_SpeakingTurnId",
                schema: "speaking",
                table: "SpeakingTurnResults",
                column: "SpeakingTurnId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingTurns_SpeakingSessionId_TurnNumber",
                schema: "speaking",
                table: "SpeakingTurns",
                columns: new[] { "SpeakingSessionId", "TurnNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationPracticeResults",
                schema: "speaking");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "speaking");

            migrationBuilder.DropTable(
                name: "SpeakingSessionSummaries",
                schema: "speaking");

            migrationBuilder.DropTable(
                name: "SpeakingTurnResults",
                schema: "speaking");

            migrationBuilder.DropTable(
                name: "SpeakingTurns",
                schema: "speaking");

            migrationBuilder.DropTable(
                name: "SpeakingSessions",
                schema: "speaking");
        }
    }
}
