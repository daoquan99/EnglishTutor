using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Commands.DeletePermission;

public sealed record DeleteAuthPermissionCommand(Guid PermissionId) : ICommand;
