using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.SetRoutingRuleActive;

public sealed record SetRoutingRuleActiveCommand(
    Guid Id,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
