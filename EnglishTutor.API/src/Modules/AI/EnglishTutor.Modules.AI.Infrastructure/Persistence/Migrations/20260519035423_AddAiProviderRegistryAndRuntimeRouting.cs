using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiProviderRegistryAndRuntimeRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiProviders",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApiKeySecretName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiRuntimeRoutes",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capability = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreferredProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreferredModelCode = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FallbackProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FallbackModelCode = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MaxTokens = table.Column<int>(type: "integer", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiRuntimeRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiProviderModels",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelCode = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Capability = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "boolean", nullable: false),
                    MaxInputTokens = table.Column<int>(type: "integer", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "integer", nullable: false),
                    CostPerInput1KTokens = table.Column<decimal>(type: "numeric", nullable: false),
                    CostPerOutput1KTokens = table.Column<decimal>(type: "numeric", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviderModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiProviderModels_AiProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "ai",
                        principalTable: "AiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderModels_Capability_IsEnabled_Priority",
                schema: "ai",
                table: "AiProviderModels",
                columns: new[] { "Capability", "IsEnabled", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderModels_ProviderId_ModelCode_Capability",
                schema: "ai",
                table: "AiProviderModels",
                columns: new[] { "ProviderId", "ModelCode", "Capability" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiProviders_ProviderName",
                schema: "ai",
                table: "AiProviders",
                column: "ProviderName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiRuntimeRoutes_TaskType_Capability",
                schema: "ai",
                table: "AiRuntimeRoutes",
                columns: new[] { "TaskType", "Capability" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiProviderModels",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "AiRuntimeRoutes",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "AiProviders",
                schema: "ai");
        }
    }
}
