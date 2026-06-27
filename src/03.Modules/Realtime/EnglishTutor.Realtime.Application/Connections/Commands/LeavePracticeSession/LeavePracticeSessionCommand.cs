using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Realtime.Application.Connections.Commands.LeavePracticeSession;

public sealed record LeavePracticeSessionCommand(
    Guid UserId,
    Guid SessionId,
    string ConnectionId) : ICommand;
