using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.MarkMastered;

public sealed record MarkMasteredCommand(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode) : ICommand<ReviewResultResponse>;
