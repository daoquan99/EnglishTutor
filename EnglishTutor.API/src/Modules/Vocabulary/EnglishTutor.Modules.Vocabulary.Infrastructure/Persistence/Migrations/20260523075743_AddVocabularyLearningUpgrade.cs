using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyLearningUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveCorrectCount",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ExampleFillBlankAttempts",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyExampleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    UserAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    AttemptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleFillBlankAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyStudySettings",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NewWordsPerDay = table.Column<int>(type: "integer", nullable: false),
                    ReviewWordsPerDay = table.Column<int>(type: "integer", nullable: false),
                    IncludeMasteredInReview = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_VocabularyStudySettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExampleFillBlankAttempts_UserId_VocabularyExampleId",
                schema: "vocabulary",
                table: "ExampleFillBlankAttempts",
                columns: new[] { "UserId", "VocabularyExampleId" });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyStudySettings_UserId_TargetLanguageCode",
                schema: "vocabulary",
                table: "VocabularyStudySettings",
                columns: new[] { "UserId", "TargetLanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExampleFillBlankAttempts",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyStudySettings",
                schema: "vocabulary");

            migrationBuilder.DropColumn(
                name: "ConsecutiveCorrectCount",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");
        }
    }
}
