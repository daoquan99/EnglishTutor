using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Persistence.Repositories;

public sealed class LearningPathCardRepository(LearningContentDbContext dbContext) : ILearningPathCardRepository
{
    public async Task<IReadOnlyList<UserLearningPathCard>> ListAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var normalized = targetLanguageCode.Trim().ToLowerInvariant();
        return await dbContext.UserLearningPathCards
            .Where(card => card.UserId == userId && card.TargetLanguageCode == normalized)
            .OrderBy(card => card.Order)
            .ToListAsync(cancellationToken);
    }

    public Task<UserLearningPathCard?> GetByContentAsync(
        Guid userId,
        ContentType contentType,
        Guid contentId,
        CancellationToken cancellationToken) =>
        dbContext.UserLearningPathCards.SingleOrDefaultAsync(
            card => card.UserId == userId &&
                card.ContentType == contentType &&
                card.ContentId == contentId,
            cancellationToken);

    public Task<UserLearningPathCard?> GetNextAsync(Guid userId, string targetLanguageCode, int order, CancellationToken cancellationToken)
    {
        var normalized = targetLanguageCode.Trim().ToLowerInvariant();
        return dbContext.UserLearningPathCards
            .Where(card => card.UserId == userId && card.TargetLanguageCode == normalized && card.Order > order)
            .OrderBy(card => card.Order)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(UserLearningPathCard card, CancellationToken cancellationToken) =>
        await dbContext.UserLearningPathCards.AddAsync(card, cancellationToken);
}
