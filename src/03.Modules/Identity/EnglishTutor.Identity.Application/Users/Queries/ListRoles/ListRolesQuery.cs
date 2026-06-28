using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Identity.Application.Users.Queries.ListRoles;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleResult>>;
