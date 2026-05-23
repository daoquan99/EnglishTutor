using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Commands.CreatePermission;

public sealed record CreateAuthPermissionCommand(string Code, string Description) : ICommand<AuthPermissionResponse>;
