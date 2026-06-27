using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.UpdateRoutingRule;

public sealed record UpdateRoutingRuleCommand(
    Guid Id,
    string Name,
    string ActivityType,
    string TopicCode,
    string ScenarioCode,
    Guid PrimaryModelId,
    Guid? FallbackModelId,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
