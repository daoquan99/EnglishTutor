using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLiveTutor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportedModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConversationMode",
                table: "Conversations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomSystemPrompt",
                table: "Conversations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Conversations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversationMode",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CustomSystemPrompt",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "Conversations");
        }
    }
}
