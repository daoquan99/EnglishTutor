using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Mistakes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mistakes");

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "mistakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    HandlerName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MistakeCategories",
                schema: "mistakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MistakeCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MistakeReviews",
                schema: "mistakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MistakeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WasCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    AnswerText = table.Column<string>(type: "text", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MistakeReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mistakes",
                schema: "mistakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NativeLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExplanationLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    OriginalText = table.Column<string>(type: "text", nullable: false),
                    CorrectedText = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NextReviewAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MasteredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mistakes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "mistakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedBy = table.Column<string>(type: "text", nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMistakeCards",
                schema: "mistakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MistakeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMistakeCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId_HandlerName",
                schema: "mistakes",
                table: "InboxMessages",
                columns: new[] { "EventId", "HandlerName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MistakeCategories_Name",
                schema: "mistakes",
                table: "MistakeCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mistakes_UserId_NextReviewAtUtc",
                schema: "mistakes",
                table: "Mistakes",
                columns: new[] { "UserId", "NextReviewAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextRetryAtUtc",
                schema: "mistakes",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextRetryAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "mistakes");

            migrationBuilder.DropTable(
                name: "MistakeCategories",
                schema: "mistakes");

            migrationBuilder.DropTable(
                name: "MistakeReviews",
                schema: "mistakes");

            migrationBuilder.DropTable(
                name: "Mistakes",
                schema: "mistakes");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "mistakes");

            migrationBuilder.DropTable(
                name: "UserMistakeCards",
                schema: "mistakes");
        }
    }
}
