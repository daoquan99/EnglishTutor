namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string? DisplayName,
    IReadOnlyList<string>? Roles,
    bool? IsActive);
