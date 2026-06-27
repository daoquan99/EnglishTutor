using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.UpdateProvider;

public sealed record UpdateProviderCommand(
    Guid Id,
    string Name,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
