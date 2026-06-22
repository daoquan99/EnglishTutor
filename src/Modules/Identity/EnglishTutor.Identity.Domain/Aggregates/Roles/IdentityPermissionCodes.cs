namespace EnglishTutor.Identity.Domain.Aggregates.Roles;

/// <summary>
/// Single source of truth for Identity permission codes. These strings are
/// the canonical values persisted in <c>identity.permissions.Code</c> and
/// emitted as authorization claim values.
/// <para>
/// <b>Why constants, not enums:</b> permission codes are persisted as strings
/// and referenced by name in policies, claims, admin UIs, API documentation
/// and cross-module permission checks. Static <c>const string</c> values are
/// the simplest representation that is both greppable and stable across
/// refactors; an enum would force casts and boxing without buying anything.
/// </para>
/// </summary>
public static class IdentityPermissionCodes
{
    /// <summary>Manage identity (create / update / delete users, roles, permissions).</summary>
    public const string Manage = "Auth:Manage";

    /// <summary>View identity data (users, roles, permissions) without modifying it.</summary>
    public const string View = "Auth:View";

    /// <summary>Manage own account (profile, password, devices).</summary>
    public const string SelfManage = "Auth:SelfManage";
}
