namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

/// <summary>
/// Response for the <c>/api/me</c> endpoint. NEVER includes audit fields,
/// soft-delete fields, password hash, or any other sensitive data.
/// </summary>
public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles);
