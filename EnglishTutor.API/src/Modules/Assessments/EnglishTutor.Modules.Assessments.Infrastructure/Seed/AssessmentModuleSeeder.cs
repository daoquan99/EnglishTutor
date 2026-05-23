using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.Assessments.Infrastructure.Seed;

public sealed class AssessmentModuleSeeder(AssessmentDataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => 70;
    public string ModuleName => "Assessments";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
