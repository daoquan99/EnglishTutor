using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLearningContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "learningcontent");

            migrationBuilder.CreateTable(
                name: "ConversationScenarios",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Level = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Setting = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationScenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    HandlerName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Level = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Skill = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "learningcontent",
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
                name: "Quizzes",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QuestionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SentencePatterns",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Level = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Pattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Examples = table.Column<string>(type: "jsonb", nullable: false),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentencePatterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLearningPathCards",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Skill = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningPathCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationLines",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Speaker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpectedResponseHint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AudioUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationLines_ConversationScenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalSchema: "learningcontent",
                        principalTable: "ConversationScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationScenarioTranslations",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Setting = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationScenarioTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationScenarioTranslations_ConversationScenarios_Scen~",
                        column: x => x.ScenarioId,
                        principalSchema: "learningcontent",
                        principalTable: "ConversationScenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonSections",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonSections_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalSchema: "learningcontent",
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonTranslations",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonTranslations_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalSchema: "learningcontent",
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationLineTranslations",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpectedResponseHint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationLineTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationLineTranslations_ConversationLines_Conversation~",
                        column: x => x.ConversationLineId,
                        principalSchema: "learningcontent",
                        principalTable: "ConversationLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonSectionTranslations",
                schema: "learningcontent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonSectionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonSectionTranslations_LessonSections_LessonSectionId",
                        column: x => x.LessonSectionId,
                        principalSchema: "learningcontent",
                        principalTable: "LessonSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationLines_ScenarioId_Order",
                schema: "learningcontent",
                table: "ConversationLines",
                columns: new[] { "ScenarioId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationLineTranslations_ConversationLineId_LanguageCode",
                schema: "learningcontent",
                table: "ConversationLineTranslations",
                columns: new[] { "ConversationLineId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationScenarios_TargetLanguageCode_Level_IsPublished",
                schema: "learningcontent",
                table: "ConversationScenarios",
                columns: new[] { "TargetLanguageCode", "Level", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationScenarioTranslations_ScenarioId_LanguageCode",
                schema: "learningcontent",
                table: "ConversationScenarioTranslations",
                columns: new[] { "ScenarioId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId_HandlerName",
                schema: "learningcontent",
                table: "InboxMessages",
                columns: new[] { "EventId", "HandlerName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TargetLanguageCode_Level_IsPublished",
                schema: "learningcontent",
                table: "Lessons",
                columns: new[] { "TargetLanguageCode", "Level", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_Topic_Skill",
                schema: "learningcontent",
                table: "Lessons",
                columns: new[] { "Topic", "Skill" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonSections_LessonId_Order",
                schema: "learningcontent",
                table: "LessonSections",
                columns: new[] { "LessonId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonSectionTranslations_LessonSectionId_LanguageCode",
                schema: "learningcontent",
                table: "LessonSectionTranslations",
                columns: new[] { "LessonSectionId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonTranslations_LessonId_LanguageCode",
                schema: "learningcontent",
                table: "LessonTranslations",
                columns: new[] { "LessonId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextRetryAtUtc",
                schema: "learningcontent",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextRetryAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SentencePatterns_TargetLanguageCode_Level",
                schema: "learningcontent",
                table: "SentencePatterns",
                columns: new[] { "TargetLanguageCode", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningPathCards_UserId_ContentType_ContentId",
                schema: "learningcontent",
                table: "UserLearningPathCards",
                columns: new[] { "UserId", "ContentType", "ContentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningPathCards_UserId_TargetLanguageCode_Order",
                schema: "learningcontent",
                table: "UserLearningPathCards",
                columns: new[] { "UserId", "TargetLanguageCode", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationLineTranslations",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "ConversationScenarioTranslations",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "LessonSectionTranslations",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "LessonTranslations",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "Quizzes",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "SentencePatterns",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "UserLearningPathCards",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "ConversationLines",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "LessonSections",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "ConversationScenarios",
                schema: "learningcontent");

            migrationBuilder.DropTable(
                name: "Lessons",
                schema: "learningcontent");
        }
    }
}
