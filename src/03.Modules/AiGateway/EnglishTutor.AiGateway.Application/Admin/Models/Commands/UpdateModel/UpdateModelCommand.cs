using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.UpdateModel;

public sealed record UpdateModelCommand(
    Guid Id,
    string DisplayName,
    string ProviderModelId,
    string[] Capabilities,
    bool? ThinkingEnabled,
    string Lifecycle,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
