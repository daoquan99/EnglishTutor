using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Notifications.Infrastructure.Seed;

public sealed class NotificationsDataSeeder(
    NotificationsDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var template in NotificationTemplateSeedData.CreateDefaultTemplates(dateTimeProvider.UtcNow))
        {
            var exists = await dbContext.NotificationTemplates.AnyAsync(
                candidate => candidate.Type == template.Type &&
                    candidate.LanguageCode == template.LanguageCode,
                cancellationToken);

            if (!exists)
            {
                dbContext.NotificationTemplates.Add(template);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
