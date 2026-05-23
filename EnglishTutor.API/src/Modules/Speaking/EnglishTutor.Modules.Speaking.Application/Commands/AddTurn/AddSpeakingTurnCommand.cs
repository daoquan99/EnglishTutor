using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Speaking.Application.Commands.AddTurn;

public sealed record AddSpeakingTurnCommand(
    Guid UserId,
    Guid SessionId,
    string? UserText,
    Stream? AudioFile = null,
    string? AudioFileName = null,
    string? AudioContentType = null) : ICommand<SpeakingTurnResponse>;
