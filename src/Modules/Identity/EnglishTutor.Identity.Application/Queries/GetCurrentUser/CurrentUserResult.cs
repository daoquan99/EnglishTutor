namespace EnglishTutor.Identity.Application.Queries.GetCurrentUser;

/// <summary>
/// Application-layer result for the get-current-user use case. The Presentation
/// endpoint (<c>MeEndpoints</c>) maps this to <c>CurrentUserResponse</c>
/// (Presentation DTO) at the HTTP boundary. Application does NOT reference
/// Presentation.
/// </summary>
public sealed record CurrentUserResult(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles);
