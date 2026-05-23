using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePreferredStudyTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreferredStudyTimeUtc",
                schema: "studyplans",
                table: "UserStudyPlans",
                newName: "PreferredStudyTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreferredStudyTime",
                schema: "studyplans",
                table: "UserStudyPlans",
                newName: "PreferredStudyTimeUtc");
        }
    }
}
