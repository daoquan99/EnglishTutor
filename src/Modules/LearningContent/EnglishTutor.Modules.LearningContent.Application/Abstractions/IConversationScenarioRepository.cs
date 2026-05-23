using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario;

namespace EnglishTutor.Modules.LearningContent.Application.Abstractions;

public interface IConversationScenarioRepository
{
    Task<ConversationScenario?> GetByIdWithDetailsAsync(Guid scenarioId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationScenario>> ListPublishedAsync(
        int page,
        int pageSize,
        string? level,
        int? difficulty,
        string? targetLanguageCode,
        CancellationToken cancellationToken);
}
