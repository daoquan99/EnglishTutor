using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Identity.Application.Users.Queries.ListPermissions;

public sealed record ListPermissionsQuery : IQuery<IReadOnlyList<PermissionResult>>;
