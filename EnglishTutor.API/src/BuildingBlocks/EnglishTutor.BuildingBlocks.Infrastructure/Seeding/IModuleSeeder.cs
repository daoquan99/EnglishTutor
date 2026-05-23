namespace EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

public interface IModuleSeeder
{
    int Order { get; }
    string ModuleName { get; }
    Task SeedAsync(CancellationToken cancellationToken = default);
}
