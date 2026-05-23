using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AI.Infrastructure.Seed;

public sealed class AiDataSeeder(
    AiDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var defaultProvider in AiSeedData.CreateDefaultProviders())
        {
            var exists = await dbContext.AiProviders.AnyAsync(
                provider => provider.ProviderName == defaultProvider.ProviderName,
                cancellationToken);

            if (!exists)
            {
                dbContext.AiProviders.Add(defaultProvider);
            }
        }

        foreach (var defaultRoute in AiSeedData.CreateDefaultRuntimeRoutes())
        {
            var exists = await dbContext.AiRuntimeRoutes.AnyAsync(
                route => route.TaskType == defaultRoute.TaskType && route.Capability == defaultRoute.Capability,
                cancellationToken);

            if (!exists)
            {
                dbContext.AiRuntimeRoutes.Add(defaultRoute);
            }
        }

        foreach (var defaultRule in AiSeedData.CreateDefaultRoutingRules())
        {
            var exists = await dbContext.ModelRoutingRules.AnyAsync(
                rule => rule.TaskType == defaultRule.TaskType,
                cancellationToken);

            if (!exists)
            {
                dbContext.ModelRoutingRules.Add(defaultRule);
            }
        }

        foreach (var defaultTemplate in AiSeedData.CreateDefaultPromptTemplates(dateTimeProvider.UtcNow))
        {
            var exists = await dbContext.PromptTemplates.AnyAsync(
                template => template.Name == defaultTemplate.Name,
                cancellationToken);

            if (!exists)
            {
                dbContext.PromptTemplates.Add(defaultTemplate);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
