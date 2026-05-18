using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Commands.AddTurn;

public sealed record AddSpeakingTurnCommand(
    Guid UserId,
    Guid SessionId,
    string UserText) : ICommand<SpeakingTurnResponse>;
