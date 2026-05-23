using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Worker.Outbox.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadLetterEventIdUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_EventId",
                schema: "messaging",
                table: "DeadLetterMessages",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeadLetterMessages_EventId",
                schema: "messaging",
                table: "DeadLetterMessages");
        }
    }
}
