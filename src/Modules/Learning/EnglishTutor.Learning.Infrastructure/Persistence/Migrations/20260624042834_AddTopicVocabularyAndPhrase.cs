using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Learning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicVocabularyAndPhrase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "topic_phrases",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phrase = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    phrase_normalized = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    translation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    context = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_topic_phrases", x => x.id);
                    table.ForeignKey(
                        name: "FK_topic_phrases_topics_topic_id",
                        column: x => x.topic_id,
                        principalSchema: "learning",
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topic_vocabularies",
                schema: "learning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    word = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    word_normalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    definition = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    part_of_speech = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phonetic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    example_sentence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    example_translation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_topic_vocabularies", x => x.id);
                    table.ForeignKey(
                        name: "FK_topic_vocabularies_topics_topic_id",
                        column: x => x.topic_id,
                        principalSchema: "learning",
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_topic_phrases_topic_id",
                schema: "learning",
                table: "topic_phrases",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_topic_phrases_topic_id_phrase_normalized",
                schema: "learning",
                table: "topic_phrases",
                columns: new[] { "topic_id", "phrase_normalized" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_topic_vocabularies_topic_id",
                schema: "learning",
                table: "topic_vocabularies",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_topic_vocabularies_topic_id_word_normalized",
                schema: "learning",
                table: "topic_vocabularies",
                columns: new[] { "topic_id", "word_normalized" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "topic_phrases",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "topic_vocabularies",
                schema: "learning");
        }
    }
}
