using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;

namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetTodayVocabulary;

public sealed record GetTodayVocabularyQuery(Guid UserId, string TargetLanguageCode) : IQuery<TodayVocabularyResponse>;
