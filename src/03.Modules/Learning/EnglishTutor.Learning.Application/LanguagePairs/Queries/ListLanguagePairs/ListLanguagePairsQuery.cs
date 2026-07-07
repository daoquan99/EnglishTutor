using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.LanguagePairs.Queries.ListLanguagePairs;

public sealed record ListLanguagePairsQuery(Guid UserId)
    : IQuery<IReadOnlyList<LearnerLanguagePairView>>;
