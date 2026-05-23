using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitFillBlank;

public sealed record SubmitFillBlankCommand(
    Guid UserId,
    Guid VocabularyExampleId,
    string TargetLanguageCode,
    string UserAnswer) : ICommand<FillBlankResultResponse>;
