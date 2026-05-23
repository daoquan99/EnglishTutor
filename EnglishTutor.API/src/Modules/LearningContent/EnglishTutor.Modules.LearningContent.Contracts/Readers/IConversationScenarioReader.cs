using EnglishTutor.Modules.LearningContent.Contracts.ReadModels;

namespace EnglishTutor.Modules.LearningContent.Contracts.Readers;

public interface IConversationScenarioReader
{
    Task<ConversationScenarioReadModel?> GetByIdAsync(Guid scenarioId, CancellationToken cancellationToken);
}
