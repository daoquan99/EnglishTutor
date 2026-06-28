using EnglishTutor.Identity.Domain.Aggregates.Roles;

namespace EnglishTutor.Identity.Application.Users;

internal static class UserManagementRolePolicy
{
    private static readonly HashSet<string> PrivilegedRoles =
    [
        Role.WellKnownNames.Owner,
        Role.WellKnownNames.Admin
    ];

    public static bool ContainsPrivilegedRole(IEnumerable<string> roles) =>
        roles.Any(role => PrivilegedRoles.Contains(role));
}
