using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Identity.Application.Commands.Refresh;

/// <summary>
/// Static factory helpers that produce the standard
/// <see cref="Result{T}"/> failure values for the refresh use case.
/// </summary>
public static class RefreshFailureResults
{
    public static Result<RefreshResult> InvalidRefreshToken() =>
        Result.Failure<RefreshResult>(Error.Unauthorized(
            "Identity.InvalidRefreshToken", "Invalid or expired refresh token."));

    public static Result<RefreshResult> ReuseDetected(Guid familyId) =>
        Result.Failure<RefreshResult>(Error.Unauthorized(
            "Identity.RefreshTokenReuse", $"Refresh-token reuse detected (family {familyId}). All sessions revoked."));
}
