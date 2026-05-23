using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Queries.ListRoles;

public sealed record ListAuthRolesQuery(bool IncludeDisabled) : IQuery<IReadOnlyList<AuthRoleResponse>>;
