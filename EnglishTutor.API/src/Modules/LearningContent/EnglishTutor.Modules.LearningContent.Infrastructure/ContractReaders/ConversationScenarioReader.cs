using EnglishTutor.Modules.LearningContent.Contracts.ReadModels;
using EnglishTutor.Modules.LearningContent.Contracts.Readers;
using EnglishTutor.Modules.LearningContent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.ContractReaders;

public sealed class ConversationScenarioReader(LearningContentDbContext dbContext) : IConversationScenarioReader
{
    public async Task<ConversationScenarioReadModel?> GetByIdAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        var scenario = await dbContext.ConversationScenarios
            .Include(candidate => candidate.Lines)
            .SingleOrDefaultAsync(candidate => candidate.Id == scenarioId && candidate.IsPublished, cancellationToken);

        if (scenario is null)
        {
            return null;
        }

        return new ConversationScenarioReadModel(
            scenario.Id,
            scenario.Title,
            scenario.Setting,
            scenario.Level.ToString(),
            scenario.Difficulty,
            scenario.Lines
                .OrderBy(line => line.Order)
                .Select(line => new ConversationLineReadModel(
                    line.Order,
                    line.Speaker.ToString(),
                    line.Text,
                    line.ExpectedResponseHint,
                    line.AudioUrl))
                .ToArray());
    }
}
