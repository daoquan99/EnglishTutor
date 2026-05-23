using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetPermission;

public sealed record GetAuthPermissionQuery(Guid PermissionId) : IQuery<AuthPermissionResponse>;
