using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.UpdateModel;

public sealed record UpdateModelCommand(
    Guid Id,
    string Name,
    string[] Capabilities,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
