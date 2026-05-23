using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.ReviewVocabulary;

public sealed record ReviewVocabularyCommand(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    bool IsCorrect,
    int Score) : ICommand<ReviewResultResponse>;
