using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMassTransitOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_message",
                schema: "aigateway");

            migrationBuilder.DropTable(
                name: "inbox_state",
                schema: "aigateway");

            migrationBuilder.DropTable(
                name: "outbox_state",
                schema: "aigateway");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_state",
                schema: "aigateway",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_state", x => x.Id);
                    table.UniqueConstraint("AK_inbox_state_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "outbox_state",
                schema: "aigateway",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_state", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message",
                schema: "aigateway",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_outbox_message_inbox_state_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalSchema: "aigateway",
                        principalTable: "inbox_state",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_outbox_message_outbox_state_OutboxId",
                        column: x => x.OutboxId,
                        principalSchema: "aigateway",
                        principalTable: "outbox_state",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_state_Delivered",
                schema: "aigateway",
                table: "inbox_state",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_EnqueueTime",
                schema: "aigateway",
                table: "outbox_message",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_ExpirationTime",
                schema: "aigateway",
                table: "outbox_message",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_InboxMessageId_InboxConsumerId_SequenceNumber",
                schema: "aigateway",
                table: "outbox_message",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_OutboxId_SequenceNumber",
                schema: "aigateway",
                table: "outbox_message",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_state_BusName_Created",
                schema: "aigateway",
                table: "outbox_state",
                columns: new[] { "BusName", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_state_Created",
                schema: "aigateway",
                table: "outbox_state",
                column: "Created");
        }
    }
}
