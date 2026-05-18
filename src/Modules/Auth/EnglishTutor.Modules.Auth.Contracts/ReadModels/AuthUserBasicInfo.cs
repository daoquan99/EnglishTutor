namespace EnglishTutor.Modules.Auth.Contracts.ReadModels;

public sealed record AuthUserBasicInfo(
    Guid UserId,
    string Email,
    string DisplayName);
