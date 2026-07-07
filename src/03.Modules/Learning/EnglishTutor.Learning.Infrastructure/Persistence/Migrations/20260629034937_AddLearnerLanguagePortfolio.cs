using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Learning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnerLanguagePortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "language_definitions",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    english_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    native_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_available_as_native = table.Column<bool>(type: "boolean", nullable: false),
                    is_available_as_target = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_language_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "learner_language_portfolios",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    active_language_pair_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_learner_language_portfolios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "learner_language_pairs",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    native_language_code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    target_language_code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    explanation_language_code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learner_language_pairs", x => x.id);
                    table.ForeignKey(
                        name: "FK_learner_language_pairs_learner_language_portfolios_portfoli~",
                        column: x => x.portfolio_id,
                        principalSchema: "learning",
                        principalTable: "learner_language_portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_language_definitions_code",
                schema: "learning",
                table: "language_definitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_language_definitions_is_active_sort_order_english_name_id",
                schema: "learning",
                table: "language_definitions",
                columns: new[] { "is_active", "sort_order", "english_name", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_learner_language_pairs_portfolio_id_native_language_code_ta~",
                schema: "learning",
                table: "learner_language_pairs",
                columns: new[] { "portfolio_id", "native_language_code", "target_language_code" },
                unique: true,
                filter: "\"status\" <> 'Archived'");

            migrationBuilder.CreateIndex(
                name: "IX_learner_language_pairs_portfolio_id_status",
                schema: "learning",
                table: "learner_language_pairs",
                columns: new[] { "portfolio_id", "status" },
                unique: true,
                filter: "\"status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_learner_language_portfolios_user_id",
                schema: "learning",
                table: "learner_language_portfolios",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "language_definitions",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "learner_language_pairs",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "learner_language_portfolios",
                schema: "learning");
        }
    }
}
