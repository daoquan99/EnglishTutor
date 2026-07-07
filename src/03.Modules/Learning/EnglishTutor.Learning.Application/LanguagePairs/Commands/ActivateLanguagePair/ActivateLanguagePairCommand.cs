using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Learning.Application.LanguagePairs.Commands.ActivateLanguagePair;

public sealed record ActivateLanguagePairCommand(Guid UserId, Guid PairId)
    : ICommand;
