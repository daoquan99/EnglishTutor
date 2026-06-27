using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Realtime.Application.Connections.Commands.RegisterHeartbeat;

public sealed record RegisterHeartbeatCommand(
    string ConnectionId) : ICommand;
