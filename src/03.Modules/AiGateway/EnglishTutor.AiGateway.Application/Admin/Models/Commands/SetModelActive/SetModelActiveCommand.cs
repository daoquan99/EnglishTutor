using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.SetModelActive;

public sealed record SetModelActiveCommand(
    Guid Id,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
