namespace EnglishTutor.Modules.Auth.Presentation.Requests;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string DisplayName);
