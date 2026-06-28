namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record UpdateUserRequest(
    string DisplayName,
    bool IsActive);
