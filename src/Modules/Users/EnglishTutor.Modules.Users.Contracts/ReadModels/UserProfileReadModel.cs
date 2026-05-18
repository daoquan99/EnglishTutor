namespace EnglishTutor.Modules.Users.Contracts.ReadModels;

public sealed record UserProfileReadModel(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio);
