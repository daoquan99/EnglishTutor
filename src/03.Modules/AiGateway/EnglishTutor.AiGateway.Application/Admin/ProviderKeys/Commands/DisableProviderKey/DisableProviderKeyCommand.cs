using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.DisableProviderKey;

public sealed record DisableProviderKeyCommand(
    Guid Id,
    Guid? ActorUserId) : ICommand;
