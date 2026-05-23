using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Persistence.Repositories;

public sealed class ConversationScenarioRepository(LearningContentDbContext dbContext) : IConversationScenarioRepository
{
    public Task<ConversationScenario?> GetByIdWithDetailsAsync(Guid scenarioId, CancellationToken cancellationToken) =>
        dbContext.ConversationScenarios
            .Include(scenario => scenario.Translations)
            .Include(scenario => scenario.Lines)
                .ThenInclude(line => line.Translations)
            .SingleOrDefaultAsync(scenario => scenario.Id == scenarioId, cancellationToken);

    public async Task<IReadOnlyList<ConversationScenario>> ListPublishedAsync(
        int page,
        int pageSize,
        string? level,
        int? difficulty,
        string? targetLanguageCode,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ConversationScenarios
            .Include(scenario => scenario.Translations)
            .Where(scenario => scenario.IsPublished);

        if (!string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            var normalized = targetLanguageCode.Trim().ToLowerInvariant();
            query = query.Where(scenario => scenario.TargetLanguageCode == normalized);
        }

        if (Enum.TryParse<LanguageLevel>(level, true, out var parsedLevel))
        {
            query = query.Where(scenario => scenario.Level == parsedLevel);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(scenario => scenario.Difficulty == difficulty.Value);
        }

        return await query
            .OrderBy(scenario => scenario.Level)
            .ThenBy(scenario => scenario.Difficulty)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
