using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;

namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudyCard;

public sealed record GetStudyCardQuery(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    string NativeLanguageCode) : IQuery<StudyCardResponse>;
