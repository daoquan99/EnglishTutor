using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Learning.Application.LanguagePairs.Commands.UpdateLanguagePairExplanation;

public sealed record UpdateLanguagePairExplanationCommand(
    Guid UserId,
    Guid PairId,
    string ExplanationLanguageCode) : ICommand;
