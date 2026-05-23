using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetRole;

public sealed record GetAuthRoleQuery(Guid RoleId) : IQuery<AuthRoleResponse>;
