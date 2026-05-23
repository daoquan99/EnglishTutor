using EnglishTutor.BuildingBlocks.Application.Abstractions;
namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetTodayVocabulary;

public sealed record GetTodayVocabularyQuery(Guid UserId, string TargetLanguageCode) : IQuery<TodayVocabularyResponse>;
