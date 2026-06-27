using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Realtime.Application.Connections.Commands.JoinPracticeSession;

public sealed record JoinPracticeSessionCommand(
    Guid UserId,
    Guid SessionId,
    string ConnectionId) : ICommand;
