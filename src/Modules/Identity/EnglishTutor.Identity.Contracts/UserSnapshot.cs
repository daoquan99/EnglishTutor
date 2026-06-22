namespace EnglishTutor.Identity.Contracts;

/// <summary>
/// Read-only projection of a user for cross-module consumers. Includes
/// roles and effective permissions so consumers do not need to query
/// the identity module more than once.
/// </summary>
public sealed record UserSnapshot(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    bool IsActive);
