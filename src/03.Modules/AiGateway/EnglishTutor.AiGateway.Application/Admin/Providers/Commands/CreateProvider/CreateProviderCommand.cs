using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider;

public sealed record CreateProviderCommand(
    string Name,
    string Code,
    bool IsActive,
    Guid? ActorUserId) : ICommand<Guid>;
