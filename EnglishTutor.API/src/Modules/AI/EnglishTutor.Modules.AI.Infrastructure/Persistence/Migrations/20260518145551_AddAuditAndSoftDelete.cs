using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "ai",
                table: "PromptVersions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "PromptVersions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "PromptVersions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "ai",
                table: "PromptTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "ai",
                table: "PromptTemplates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "ai",
                table: "PromptTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ai",
                table: "PromptTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "PromptTemplates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "PromptTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "ModelRoutingRules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "ai",
                table: "ModelRoutingRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "ModelRoutingRules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "ModelRoutingRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiUsageCounters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiUsageCounters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "AiUsageCounters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiRequestLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "ai",
                table: "AiRequestLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "ai",
                table: "AiRequestLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ai",
                table: "AiRequestLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "AiRequestLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "AiRequestLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiCostEstimations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiCostEstimations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "AiCostEstimations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "AiCostEstimations",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "ai",
                table: "PromptVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "PromptVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "PromptVersions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "ai",
                table: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "ai",
                table: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "ai",
                table: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ai",
                table: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "ModelRoutingRules");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "ai",
                table: "ModelRoutingRules");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "ModelRoutingRules");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "ModelRoutingRules");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiUsageCounters");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiUsageCounters");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "AiUsageCounters");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiRequestLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "ai",
                table: "AiRequestLogs");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "ai",
                table: "AiRequestLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ai",
                table: "AiRequestLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "AiRequestLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "AiRequestLogs");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiCostEstimations");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiCostEstimations");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "ai",
                table: "AiCostEstimations");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "ai",
                table: "AiCostEstimations");
        }
    }
}
