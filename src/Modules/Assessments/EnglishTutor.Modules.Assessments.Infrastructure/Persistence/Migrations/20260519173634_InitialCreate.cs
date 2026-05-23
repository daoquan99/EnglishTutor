using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Assessments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "assessments");

            migrationBuilder.CreateTable(
                name: "AssessmentDefinitions",
                schema: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ForLevel = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PassingScore = table.Column<int>(type: "integer", nullable: false),
                    MinSkillScore = table.Column<int>(type: "integer", nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AssessmentDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentGradingResults",
                schema: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false),
                    SectionScoresJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    GradedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GradingNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentGradingResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "assessments",
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
                name: "UserAssessmentAttempts",
                schema: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CurrentLevel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GradedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_UserAssessmentAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentRubrics",
                schema: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Skill = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Criteria = table.Column<string>(type: "text", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    ScoringGuide = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentRubrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentRubrics_AssessmentDefinitions_AssessmentDefinitio~",
                        column: x => x.AssessmentDefinitionId,
                        principalSchema: "assessments",
                        principalTable: "AssessmentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentSections",
                schema: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Skill = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentSections_AssessmentDefinitions_AssessmentDefiniti~",
                        column: x => x.AssessmentDefinitionId,
                        principalSchema: "assessments",
                        principalTable: "AssessmentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAssessmentAnswers",
                schema: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserAnswer = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Feedback = table.Column<string>(type: "text", nullable: true),
                    IsGraded = table.Column<bool>(type: "boolean", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssessmentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAssessmentAnswers_UserAssessmentAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalSchema: "assessments",
                        principalTable: "UserAssessmentAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestions",
                schema: "assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    IsAiGraded = table.Column<bool>(type: "boolean", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestions_AssessmentSections_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "assessments",
                        principalTable: "AssessmentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentDefinitions_AssessmentType_TargetLanguageCode_For~",
                schema: "assessments",
                table: "AssessmentDefinitions",
                columns: new[] { "AssessmentType", "TargetLanguageCode", "ForLevel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentGradingResults_AttemptId",
                schema: "assessments",
                table: "AssessmentGradingResults",
                column: "AttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_SectionId_Order",
                schema: "assessments",
                table: "AssessmentQuestions",
                columns: new[] { "SectionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentRubrics_AssessmentDefinitionId_Skill",
                schema: "assessments",
                table: "AssessmentRubrics",
                columns: new[] { "AssessmentDefinitionId", "Skill" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentSections_AssessmentDefinitionId_Order",
                schema: "assessments",
                table: "AssessmentSections",
                columns: new[] { "AssessmentDefinitionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextRetryAtUtc",
                schema: "assessments",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextRetryAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAnswers_AttemptId_QuestionId",
                schema: "assessments",
                table: "UserAssessmentAnswers",
                columns: new[] { "AttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAttempts_UserId_AssessmentDefinitionId",
                schema: "assessments",
                table: "UserAssessmentAttempts",
                columns: new[] { "UserId", "AssessmentDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAttempts_UserId_Status_StartedAtUtc",
                schema: "assessments",
                table: "UserAssessmentAttempts",
                columns: new[] { "UserId", "Status", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentGradingResults",
                schema: "assessments");

            migrationBuilder.DropTable(
                name: "AssessmentQuestions",
                schema: "assessments");

            migrationBuilder.DropTable(
                name: "AssessmentRubrics",
                schema: "assessments");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "assessments");

            migrationBuilder.DropTable(
                name: "UserAssessmentAnswers",
                schema: "assessments");

            migrationBuilder.DropTable(
                name: "AssessmentSections",
                schema: "assessments");

            migrationBuilder.DropTable(
                name: "UserAssessmentAttempts",
                schema: "assessments");

            migrationBuilder.DropTable(
                name: "AssessmentDefinitions",
                schema: "assessments");
        }
    }
}
