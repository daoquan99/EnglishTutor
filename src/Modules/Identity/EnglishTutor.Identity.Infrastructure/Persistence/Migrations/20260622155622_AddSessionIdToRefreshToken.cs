using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                schema: "identity",
                table: "refresh_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "identity",
                table: "refresh_tokens");
        }
    }
}
