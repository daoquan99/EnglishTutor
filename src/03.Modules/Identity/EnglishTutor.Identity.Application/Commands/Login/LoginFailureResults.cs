using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Identity.Application.Commands.Login;

/// <summary>
/// Static factory helpers that produce the standard
/// <see cref="Result{T}"/> failure values for the login use case.
/// </summary>
public static class LoginFailureResults
{
    /// <summary>Generic credential error (no email enumeration).</summary>
    public static Result<LoginResult> InvalidCredentials() =>
        Result.Failure<LoginResult>(Error.Unauthorized(
            "Identity.InvalidCredentials",
            "Invalid email or password."));

    public static Result<LoginResult> AccountLocked(DateTimeOffset lockoutEndUtc) =>
        Result.Failure<LoginResult>(Error.Unauthorized(
            "Identity.AccountLocked",
            $"Account is locked until {lockoutEndUtc:O}."));

    public static Result<LoginResult> AccountInactive() =>
        Result.Failure<LoginResult>(Error.Unauthorized(
            "Identity.AccountInactive",
            "Account is inactive."));
}
