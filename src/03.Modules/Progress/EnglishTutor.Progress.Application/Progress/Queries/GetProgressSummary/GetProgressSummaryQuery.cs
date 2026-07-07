using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Progress.Application.Progress.Queries.GetProgressSummary;

public sealed record GetProgressSummaryQuery(Guid UserId, Guid? LanguagePairId)
    : IQuery<IReadOnlyList<LearnerLanguageProgressView>>;
