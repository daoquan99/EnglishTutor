using EnglishTutor.BuildingBlocks.Application.Abstractions;
namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudyCard;

public sealed record GetStudyCardQuery(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    string NativeLanguageCode) : IQuery<StudyCardResponse>;
