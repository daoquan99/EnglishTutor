using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetMyVocabulary;

public sealed record GetMyVocabularyQuery(Guid UserId, string TargetLanguageCode) : IQuery<MyVocabularyResponse>;
