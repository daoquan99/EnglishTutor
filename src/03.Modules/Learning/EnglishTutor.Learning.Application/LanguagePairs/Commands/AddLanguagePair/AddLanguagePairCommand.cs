using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Learning.Application.LanguagePairs.Commands.AddLanguagePair;

public sealed record AddLanguagePairCommand(
    Guid UserId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode) : ICommand<Guid>;
