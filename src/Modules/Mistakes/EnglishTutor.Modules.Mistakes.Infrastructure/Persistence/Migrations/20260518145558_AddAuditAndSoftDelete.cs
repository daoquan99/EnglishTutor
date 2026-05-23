using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Mistakes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "mistakes",
                table: "UserMistakeCards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "UserMistakeCards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "UserMistakeCards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "UserMistakeCards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "Mistakes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "mistakes",
                table: "Mistakes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "mistakes",
                table: "Mistakes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "mistakes",
                table: "Mistakes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "Mistakes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "Mistakes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "mistakes",
                table: "MistakeReviews",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "MistakeReviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "MistakeReviews",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "MistakeReviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "mistakes",
                table: "MistakeCategories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "MistakeCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "MistakeCategories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "MistakeCategories",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "mistakes",
                table: "UserMistakeCards");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "UserMistakeCards");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "UserMistakeCards");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "UserMistakeCards");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "Mistakes");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "mistakes",
                table: "Mistakes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "mistakes",
                table: "Mistakes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "mistakes",
                table: "Mistakes");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "Mistakes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "Mistakes");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "mistakes",
                table: "MistakeReviews");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "MistakeReviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "MistakeReviews");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "MistakeReviews");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "mistakes",
                table: "MistakeCategories");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "mistakes",
                table: "MistakeCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "mistakes",
                table: "MistakeCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "mistakes",
                table: "MistakeCategories");
        }
    }
}
