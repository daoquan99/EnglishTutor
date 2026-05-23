using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Seed;

public sealed class LearningContentModuleSeeder(LearningContentDataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => 50;
    public string ModuleName => "LearningContent";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
