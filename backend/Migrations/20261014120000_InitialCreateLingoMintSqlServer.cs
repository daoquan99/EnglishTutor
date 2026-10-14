using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLiveTutor.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateLingoMintSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CefrLevel = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ExtendedThinking = table.Column<bool>(type: "bit", nullable: false),
                    VoiceName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SpeechSpeed = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Corrections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TurnId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OriginalText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorCategory = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExplanationVi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CefrTip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corrections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Corrections_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TurnId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Speaker = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TranscriptLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TranslationLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Corrections_ConversationId",
                table: "Corrections",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_SequenceOrder",
                table: "Messages",
                columns: new[] { "ConversationId", "SequenceOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_TurnId_Speaker",
                table: "Messages",
                columns: new[] { "ConversationId", "TurnId", "Speaker" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Corrections");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");
        }
    }
}
