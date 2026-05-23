using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncSpeakingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConversationScenarioId",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentLineOrder",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversationScenarioId",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "CurrentLineOrder",
                schema: "speaking",
                table: "SpeakingSessions");
        }
    }
}
