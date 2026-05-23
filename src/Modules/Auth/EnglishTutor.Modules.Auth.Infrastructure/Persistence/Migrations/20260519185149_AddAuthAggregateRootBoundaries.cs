using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAggregateRootBoundaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "Permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "auth",
                table: "Permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "Permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "AuthSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "auth",
                table: "AuthSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "AuthSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "AuthSecurityEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "auth",
                table: "AuthSecurityEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "AuthSecurityEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "auth",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "auth",
                table: "AuthSecurityEvents");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "auth",
                table: "AuthSecurityEvents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "AuthSecurityEvents");
        }
    }
}
