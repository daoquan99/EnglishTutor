using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyTranslations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyTranslations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyTranslations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyTranslations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "VocabularyItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "VocabularyItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "VocabularyItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExamples",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExamples",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExamples",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExamples",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "UserVocabularyMasteries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyTranslations");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyTranslations");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewSessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewSessions");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyReviewAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyPronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyItems");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "VocabularyItems");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "VocabularyItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "VocabularyItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyItems");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExampleTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExamples");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExamples");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "VocabularyExamples");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "VocabularyExamples");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "UserVocabularyMasteries");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "vocabulary",
                table: "ExampleSentencePronunciationAttempts");
        }
    }
}
