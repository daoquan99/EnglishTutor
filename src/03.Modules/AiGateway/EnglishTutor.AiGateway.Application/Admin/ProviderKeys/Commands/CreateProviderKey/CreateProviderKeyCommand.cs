using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey;

public sealed record CreateProviderKeyCommand(
    Guid ProviderId,
    string Name,
    string Secret,
    int Priority,
    bool IsActive,
    Guid? ActorUserId) : ICommand<Guid>;
