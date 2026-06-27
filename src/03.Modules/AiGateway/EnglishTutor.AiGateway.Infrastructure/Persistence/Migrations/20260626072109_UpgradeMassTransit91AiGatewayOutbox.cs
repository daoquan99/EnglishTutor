using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeMassTransit91AiGatewayOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusName",
                schema: "aigateway",
                table: "outbox_state",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_state_BusName_Created",
                schema: "aigateway",
                table: "outbox_state",
                columns: new[] { "BusName", "Created" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_state_BusName_Created",
                schema: "aigateway",
                table: "outbox_state");

            migrationBuilder.DropColumn(
                name: "BusName",
                schema: "aigateway",
                table: "outbox_state");
        }
    }
}
