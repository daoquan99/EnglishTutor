using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;

namespace EnglishTutor.Modules.LearningContent.Application.Abstractions;

public interface ILearningPathCardRepository
{
    Task<IReadOnlyList<UserLearningPathCard>> ListAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);

    Task<UserLearningPathCard?> GetByContentAsync(
        Guid userId,
        ContentType contentType,
        Guid contentId,
        CancellationToken cancellationToken);

    Task<UserLearningPathCard?> GetNextAsync(Guid userId, string targetLanguageCode, int order, CancellationToken cancellationToken);

    Task AddAsync(UserLearningPathCard card, CancellationToken cancellationToken);
}
