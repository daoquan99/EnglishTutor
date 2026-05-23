using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "auth",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "auth",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE auth."UserCredentials"
                SET "UpdatedAtUtc" = "CreatedAtUtc"
                WHERE "UpdatedAtUtc" IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "UserCredentials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "auth",
                table: "UserCredentials",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "UserCredentials",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "auth",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "auth",
                table: "AuthSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "AuthSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "AuthSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "auth",
                table: "AuthSecurityEvents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "auth",
                table: "AuthSecurityEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "AuthSecurityEvents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "AuthSecurityEvents",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "auth",
                table: "UserCredentials");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "UserCredentials");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "auth",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "auth",
                table: "AuthSecurityEvents");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "auth",
                table: "AuthSecurityEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "AuthSecurityEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "auth",
                table: "AuthSecurityEvents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "auth",
                table: "UserCredentials",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
