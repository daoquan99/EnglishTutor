using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Seed;

public sealed class VocabularyModuleSeeder(VocabularyDataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => 30;
    public string ModuleName => "Vocabulary";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
