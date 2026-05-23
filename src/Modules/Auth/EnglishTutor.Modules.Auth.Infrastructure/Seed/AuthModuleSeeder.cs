using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.Auth.Infrastructure.Seed;

public sealed class AuthModuleSeeder(AuthDataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => 10;
    public string ModuleName => "Auth";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
