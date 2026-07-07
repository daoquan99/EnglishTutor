using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Progress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialProgressLanguageProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "progress");

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "progress",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContractName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => new { x.MessageId, x.ConsumerName });
                });

            migrationBuilder.CreateTable(
                name: "learner_language_progress",
                schema: "progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_pair_id = table.Column<Guid>(type: "uuid", nullable: false),
                    native_language_code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    target_language_code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    sessions_completed = table.Column<int>(type: "integer", nullable: false),
                    speaking_seconds = table.Column<long>(type: "bigint", nullable: false),
                    feedback_count = table.Column<int>(type: "integer", nullable: false),
                    latest_score = table.Column<int>(type: "integer", nullable: true),
                    current_cefr_level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    last_practiced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learner_language_progress", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_CompletedAtUtc",
                schema: "progress",
                table: "InboxMessages",
                column: "CompletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_learner_language_progress_user_id_language_pair_id",
                schema: "progress",
                table: "learner_language_progress",
                columns: new[] { "user_id", "language_pair_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "learner_language_progress",
                schema: "progress");
        }
    }
}
