using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Practice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeLanguageSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "explanation_language_code",
                schema: "practice",
                table: "practice_sessions",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<Guid>(
                name: "language_pair_id",
                schema: "practice",
                table: "practice_sessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<long>(
                name: "language_pair_version",
                schema: "practice",
                table: "practice_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<string>(
                name: "native_language_code",
                schema: "practice",
                table: "practice_sessions",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<string>(
                name: "target_language_code",
                schema: "practice",
                table: "practice_sessions",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "explanation_language_code",
                schema: "practice",
                table: "practice_sessions");

            migrationBuilder.DropColumn(
                name: "language_pair_id",
                schema: "practice",
                table: "practice_sessions");

            migrationBuilder.DropColumn(
                name: "language_pair_version",
                schema: "practice",
                table: "practice_sessions");

            migrationBuilder.DropColumn(
                name: "native_language_code",
                schema: "practice",
                table: "practice_sessions");

            migrationBuilder.DropColumn(
                name: "target_language_code",
                schema: "practice",
                table: "practice_sessions");
        }
    }
}
