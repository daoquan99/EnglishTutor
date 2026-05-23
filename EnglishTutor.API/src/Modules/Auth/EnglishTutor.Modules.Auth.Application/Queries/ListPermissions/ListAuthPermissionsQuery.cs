using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Queries.ListPermissions;

public sealed record ListAuthPermissionsQuery(bool IncludeDisabled) : IQuery<IReadOnlyList<AuthPermissionResponse>>;
