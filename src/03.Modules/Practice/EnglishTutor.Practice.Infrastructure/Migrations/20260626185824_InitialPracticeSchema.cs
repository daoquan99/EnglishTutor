using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Practice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPracticeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "practice");

            migrationBuilder.CreateTable(
                name: "practice_sessions",
                schema: "practice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quota_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_lease_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    end_reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scenario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    topic_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mode_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    learner_facing_instructions = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_practice_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scenario_read_models",
                schema: "practice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    topic_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mode_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    learner_facing_instructions = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenario_read_models", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "session_events",
                schema: "practice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    practice_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_session_events_practice_sessions_practice_session_id",
                        column: x => x.practice_session_id,
                        principalSchema: "practice",
                        principalTable: "practice_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transcript_messages",
                schema: "practice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    practice_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transcript_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_transcript_messages_practice_sessions_practice_session_id",
                        column: x => x.practice_session_id,
                        principalSchema: "practice",
                        principalTable: "practice_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_practice_sessions_user_id",
                schema: "practice",
                table: "practice_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_practice_sessions_user_id_started_at",
                schema: "practice",
                table: "practice_sessions",
                columns: new[] { "user_id", "started_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_session_events_session_id",
                schema: "practice",
                table: "session_events",
                column: "practice_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_transcript_messages_session_seq",
                schema: "practice",
                table: "transcript_messages",
                columns: new[] { "practice_session_id", "sequence_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scenario_read_models",
                schema: "practice");

            migrationBuilder.DropTable(
                name: "session_events",
                schema: "practice");

            migrationBuilder.DropTable(
                name: "transcript_messages",
                schema: "practice");

            migrationBuilder.DropTable(
                name: "practice_sessions",
                schema: "practice");
        }
    }
}
