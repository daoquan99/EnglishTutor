using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Progress.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyTokens : Migration
    {
        // xmin is a Postgres system column that always exists on every table. The model snapshot
        // declares the "xmin" shadow property as a concurrency token so EF includes it in UPDATE
        // WHERE clauses; no DDL is required to create the column.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Concurrency tokens are added via shadow properties; no schema change to revert.
        }
    }
}
