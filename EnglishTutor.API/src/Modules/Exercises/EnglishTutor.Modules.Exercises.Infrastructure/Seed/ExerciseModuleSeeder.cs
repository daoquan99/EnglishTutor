using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.Exercises.Infrastructure.Seed;

public sealed class ExerciseModuleSeeder(ExerciseDataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => 60;
    public string ModuleName => "Exercises";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
