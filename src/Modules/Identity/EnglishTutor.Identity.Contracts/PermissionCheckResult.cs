namespace EnglishTutor.Identity.Contracts;

/// <summary>
/// Result of a permission check. <see cref="Allowed"/> is the only field
/// callers should branch on. <see cref="Reason"/> is for internal logging
/// and audit — never expose it to API consumers.
/// </summary>
public sealed record PermissionCheckResult(bool Allowed, string? Reason = null)
{
    public static PermissionCheckResult Allow() => new(true);
    public static PermissionCheckResult Deny(string reason) => new(false, reason);
}
