using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Learning.Application.LanguagePairs.Commands.ArchiveLanguagePair;

public sealed record ArchiveLanguagePairCommand(Guid UserId, Guid PairId)
    : ICommand;
