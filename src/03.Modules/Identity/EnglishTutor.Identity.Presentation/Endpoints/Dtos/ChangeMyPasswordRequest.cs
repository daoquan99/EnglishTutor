namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record ChangeMyPasswordRequest(
    string CurrentPassword,
    string NewPassword);
