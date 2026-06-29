using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;

public sealed record CreateModelCommand(
    Guid ProviderId,
    string DisplayName,
    string Code,
    string ProviderModelId,
    string[] Capabilities,
    bool? ThinkingEnabled,
    string Lifecycle,
    bool IsActive,
    Guid? ActorUserId) : ICommand<Guid>
{
    public CreateModelCommand(
        Guid providerId,
        string name,
        string code,
        string[] capabilities,
        bool isActive,
        Guid? actorUserId)
        : this(
            providerId,
            name,
            code,
            code,
            capabilities.Length == 0 ? ["content-generation"] : capabilities,
            null,
            "Stable",
            isActive,
            actorUserId)
    {
    }
}
