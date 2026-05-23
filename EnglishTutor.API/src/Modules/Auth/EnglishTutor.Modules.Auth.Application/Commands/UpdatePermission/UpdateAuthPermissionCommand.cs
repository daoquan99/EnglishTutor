using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdatePermission;

public sealed record UpdateAuthPermissionCommand(Guid PermissionId, string Description, bool IsEnabled)
    : ICommand<AuthPermissionResponse>;
