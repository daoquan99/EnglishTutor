using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserTargetLanguages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "users",
                table: "UserTargetLanguages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "users",
                table: "UserTargetLanguages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "UserTargetLanguages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserTargetLanguages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "users",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "users",
                table: "UserProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "UserProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "users",
                table: "UserPreferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserPreferences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserPreferences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserLanguageSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "users",
                table: "UserLanguageSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "users",
                table: "UserLanguageSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "UserLanguageSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserLanguageSettings",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserTargetLanguages");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "users",
                table: "UserTargetLanguages");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "users",
                table: "UserTargetLanguages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "users",
                table: "UserTargetLanguages");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserTargetLanguages");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "users",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "users",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "users",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "users",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "users",
                table: "UserLanguageSettings");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "users",
                table: "UserLanguageSettings");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "users",
                table: "UserLanguageSettings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "users",
                table: "UserLanguageSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "users",
                table: "UserLanguageSettings");
        }
    }
}
