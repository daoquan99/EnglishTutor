namespace EnglishTutor.Identity.Domain;

/// <summary>
/// Module names persisted in <c>identity.permissions.ModuleName</c>. A
/// permission code's <c>ModuleName</c> groups permissions by the module
/// that owns them and is used by admin UIs and cross-module permission
/// filtering. Persisted as a string, so it lives as <c>const string</c>
/// for the same reason <see cref="Aggregates.Roles.IdentityPermissionCodes"/>
/// does.
/// </summary>
public static class IdentityModuleNames
{
    /// <summary>Identity module — users, roles, permissions, sessions.</summary>
    public const string Identity = "Identity";
}
