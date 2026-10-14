using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Vocabulary;

public sealed record GetVocabularyFlashcardQuery(
    Guid Id,
    Guid? SenseId,
    string UserId = "default-learner"
) : IQuery<VocabularyFlashcardDto?>;

public sealed class GetVocabularyFlashcardQueryHandler : IQueryHandler<GetVocabularyFlashcardQuery, VocabularyFlashcardDto?>
{
    private readonly IAppDbContextFactory _dbFactory;

    public GetVocabularyFlashcardQueryHandler(IAppDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<VocabularyFlashcardDto?> HandleAsync(GetVocabularyFlashcardQuery query, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await VocabularyProjectionHelper.GetFlashcardByVocabularyIdAsync(
            db,
            query.Id,
            query.SenseId,
            query.UserId,
            ct);
    }
}
