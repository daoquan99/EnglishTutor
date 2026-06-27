using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Identity.Presentation.Auth;

/// <summary>
/// Builds RFC ProblemDetails results for auth endpoints with a stable
/// <c>errorCode</c> extension and safe titles/details (no token state, no
/// secrets, no stack traces). See Batch R1 API error contract and
/// <c>api-contracts.md</c>.
/// </summary>
public static class AuthProblemResults
{
    public static IResult Problem(int statusCode, string errorCode, string title, string detail) =>
        ApiResults.Problem(
            statusCode: statusCode,
            code: errorCode,
            title: title,
            message: detail);

    public static IResult RefreshCookieMissing() =>
        Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.RefreshCookieMissing,
            "Refresh cookie missing", "No refresh credential was presented.");

    public static IResult CsrfMissing() =>
        Problem(StatusCodes.Status403Forbidden, AuthErrorCodes.CsrfMissing,
            "CSRF token missing", "A required anti-forgery token was not provided.");

    public static IResult CsrfInvalid() =>
        Problem(StatusCodes.Status403Forbidden, AuthErrorCodes.CsrfInvalid,
            "CSRF token invalid", "The anti-forgery token did not validate.");

    public static IResult Unauthorized() =>
        Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.Unauthorized,
            "Unauthorized", "Authentication is required.");

    /// <summary>
    /// Maps a Domain <see cref="Error"/> from an auth use case to a stable
    /// ProblemDetails response. All refresh/credential failures collapse to a
    /// single generic 401 so callers cannot distinguish token-state from
    /// user-state (no enumeration).
    /// </summary>
    public static IResult FromError(Error error) => error.Code switch
    {
        "Identity.InvalidCredentials"  => Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.Unauthorized, "Unauthorized", "Invalid credentials."),
        "Identity.AccountLocked"       => Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.Unauthorized, "Unauthorized", "Invalid credentials."),
        "Identity.AccountInactive"     => Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.Unauthorized, "Unauthorized", "Invalid credentials."),
        "Identity.RefreshTokenReuse"   => Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.RefreshInvalid, "Unauthorized", "Invalid or expired refresh token."),
        "Identity.InvalidRefreshToken" => Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.RefreshInvalid, "Unauthorized", "Invalid or expired refresh token."),
        "Identity.UserNotFound"        => Problem(StatusCodes.Status401Unauthorized, AuthErrorCodes.RefreshInvalid, "Unauthorized", "Invalid or expired refresh token."),
        "Identity.SessionNotFound"     => Problem(StatusCodes.Status404NotFound, "auth.session.not_found", "Not found", "Session not found."),
        _ => Problem(StatusCodes.Status400BadRequest, "auth.request.invalid", "Bad request", "The request could not be processed."),
    };
}
