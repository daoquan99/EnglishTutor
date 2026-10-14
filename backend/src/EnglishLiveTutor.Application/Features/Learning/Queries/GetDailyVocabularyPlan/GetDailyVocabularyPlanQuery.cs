using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Learning;

/// <summary>
/// Note (CQRS pragmatic side-effect): Although served via HTTP GET (/api/learning/flashcards/today),
/// this operation ensures the daily learning plan exists, carries over pending past items, and schedules
/// reviews. It delegates to InitializeDailyVocabularyPlanCommandHandler.
/// </summary>

public sealed record GetDailyVocabularyPlanQuery(
    string UserId,
    string? TimeZoneId,
    int? Target
) : IQuery<DailyVocabularyPlanResponseDto>;

public sealed class GetDailyVocabularyPlanQueryHandler : IQueryHandler<GetDailyVocabularyPlanQuery, DailyVocabularyPlanResponseDto>
{
    private readonly ICommandHandler<InitializeDailyVocabularyPlanCommand, DailyVocabularyPlanResponseDto> _initHandler;

    public GetDailyVocabularyPlanQueryHandler(
        ICommandHandler<InitializeDailyVocabularyPlanCommand, DailyVocabularyPlanResponseDto> initHandler)
    {
        _initHandler = initHandler;
    }

    public async Task<DailyVocabularyPlanResponseDto> HandleAsync(GetDailyVocabularyPlanQuery query, CancellationToken ct = default)
    {
        return await _initHandler.HandleAsync(
            new InitializeDailyVocabularyPlanCommand(
                query.UserId,
                new InitializeDailyPlanRequest(
                    ApiKey: null,
                    NewVocabularyTarget: query.Target ?? 20,
                    TimeZoneId: query.TimeZoneId ?? "Asia/Ho_Chi_Minh",
                    GenerateWithAiIfNeeded: false
                )
            ),
            ct);
    }
}
