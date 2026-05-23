using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RepairPreferredStudyTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'studyplans'
                          AND table_name = 'UserStudyPlans'
                          AND column_name = 'PreferredStudyTimeUtc')
                       AND NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'studyplans'
                          AND table_name = 'UserStudyPlans'
                          AND column_name = 'PreferredStudyTime')
                    THEN
                        ALTER TABLE studyplans."UserStudyPlans"
                            RENAME COLUMN "PreferredStudyTimeUtc" TO "PreferredStudyTime";
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'studyplans'
                          AND table_name = 'UserStudyPlans'
                          AND column_name = 'PreferredStudyTime')
                       AND NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'studyplans'
                          AND table_name = 'UserStudyPlans'
                          AND column_name = 'PreferredStudyTimeUtc')
                    THEN
                        ALTER TABLE studyplans."UserStudyPlans"
                            RENAME COLUMN "PreferredStudyTime" TO "PreferredStudyTimeUtc";
                    END IF;
                END $$;
                """);
        }
    }
}
