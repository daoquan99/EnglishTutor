using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Commands.CreateRole;

public sealed record CreateAuthRoleCommand(
    string Name,
    string Description,
    bool IsEnabled,
    IReadOnlyCollection<Guid> PermissionIds) : ICommand<AuthRoleResponse>;
