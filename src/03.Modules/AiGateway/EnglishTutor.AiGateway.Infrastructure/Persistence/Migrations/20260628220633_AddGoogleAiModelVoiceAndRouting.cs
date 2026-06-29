using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAiModelVoiceAndRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                schema: "aigateway",
                table: "ai_models",
                newName: "provider_model_id");

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "aigateway",
                table: "ai_models",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lifecycle",
                schema: "aigateway",
                table: "ai_models",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Stable");

            migrationBuilder.AddColumn<bool>(
                name: "thinking_enabled",
                schema: "aigateway",
                table: "ai_models",
                type: "boolean",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE aigateway.ai_models
                SET display_name = provider_model_id
                WHERE display_name IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "display_name",
                schema: "aigateway",
                table: "ai_models",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ai_model_voices",
                schema: "aigateway",
                columns: table => new
                {
                    model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_model_voices", x => new { x.model_id, x.voice_id });
                    table.ForeignKey(
                        name: "FK_ai_model_voices_ai_models_model_id",
                        column: x => x.model_id,
                        principalSchema: "aigateway",
                        principalTable: "ai_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_voices",
                schema: "aigateway",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voice_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    style = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gender = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ai_voices", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_models_provider_id_is_active_lifecycle",
                schema: "aigateway",
                table: "ai_models",
                columns: new[] { "provider_id", "is_active", "lifecycle" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_models_provider_id_provider_model_id",
                schema: "aigateway",
                table: "ai_models",
                columns: new[] { "provider_id", "provider_model_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_model_voices_model_id_is_default",
                schema: "aigateway",
                table: "ai_model_voices",
                columns: new[] { "model_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_voices_provider_id_is_active_display_name_id",
                schema: "aigateway",
                table: "ai_voices",
                columns: new[] { "provider_id", "is_active", "display_name", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_voices_provider_id_voice_id",
                schema: "aigateway",
                table: "ai_voices",
                columns: new[] { "provider_id", "voice_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_model_voices",
                schema: "aigateway");

            migrationBuilder.DropTable(
                name: "ai_voices",
                schema: "aigateway");

            migrationBuilder.DropIndex(
                name: "IX_ai_models_provider_id_is_active_lifecycle",
                schema: "aigateway",
                table: "ai_models");

            migrationBuilder.DropIndex(
                name: "IX_ai_models_provider_id_provider_model_id",
                schema: "aigateway",
                table: "ai_models");

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "aigateway",
                table: "ai_models");

            migrationBuilder.DropColumn(
                name: "lifecycle",
                schema: "aigateway",
                table: "ai_models");

            migrationBuilder.DropColumn(
                name: "thinking_enabled",
                schema: "aigateway",
                table: "ai_models");

            migrationBuilder.RenameColumn(
                name: "provider_model_id",
                schema: "aigateway",
                table: "ai_models",
                newName: "name");
        }
    }
}
