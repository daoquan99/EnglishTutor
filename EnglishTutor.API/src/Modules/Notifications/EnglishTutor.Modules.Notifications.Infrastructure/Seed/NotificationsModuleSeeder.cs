using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;

namespace EnglishTutor.Modules.Notifications.Infrastructure.Seed;

public sealed class NotificationsModuleSeeder(NotificationsDataSeeder dataSeeder) : IModuleSeeder
{
    public int Order => 40;
    public string ModuleName => "Notifications";

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        dataSeeder.SeedAsync(cancellationToken);
}
