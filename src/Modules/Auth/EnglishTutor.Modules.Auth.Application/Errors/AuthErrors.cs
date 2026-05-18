using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Auth.Application.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Invalid email or password.");

    public static readonly Error EmailAlreadyExists =
        Error.Conflict("Email already exists.");

    public static readonly Error RefreshTokenExpired =
        Error.Unauthorized("Refresh token has expired.");

    public static readonly Error RefreshTokenRevoked =
        Error.Unauthorized("Refresh token has been revoked.");

    public static readonly Error RefreshTokenNotFound =
        Error.NotFound("Refresh token", "provided");

    public static readonly Error AuthSessionNotFound =
        Error.NotFound("Auth session", "provided");

    public static readonly Error RefreshTokenSuspicious =
        Error.Unauthorized("Refresh token session is suspicious and has been revoked.");

    public static readonly Error UserInactive =
        Error.Forbidden("User is inactive.");

    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("Auth user", userId);
}
