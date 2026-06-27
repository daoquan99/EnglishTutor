using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;

public sealed record CreateModelCommand(
    Guid ProviderId,
    string Name,
    string Code,
    string[] Capabilities,
    bool IsActive,
    Guid? ActorUserId) : ICommand<Guid>;
