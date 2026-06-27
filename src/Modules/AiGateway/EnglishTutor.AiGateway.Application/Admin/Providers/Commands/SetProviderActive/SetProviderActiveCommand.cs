using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.SetProviderActive;

public sealed record SetProviderActiveCommand(
    Guid Id,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
