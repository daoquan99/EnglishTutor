using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.AI.Infrastructure.Seed;

public sealed class AiModuleSeeder(AiDataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => 20;
    public string ModuleName => "AI";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
