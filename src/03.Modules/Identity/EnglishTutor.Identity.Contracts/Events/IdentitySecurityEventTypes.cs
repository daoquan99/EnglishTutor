namespace EnglishTutor.Identity.Contracts.Events;

/// <summary>
/// Stable discriminator codes for <see cref="IdentitySecurityEventRecordedV1.EventType"/>.
/// These map to the candidate events named in the Batch R1 Phase 1 design.
/// </summary>
public static class IdentitySecurityEventTypes
{
    public const string LoginSucceeded = "login_succeeded";
    public const string LoginFailed = "login_failed";
    public const string RefreshSucceeded = "refresh_succeeded";
    public const string RefreshTokenReuseDetected = "refresh_token_reuse_detected";
    public const string RefreshRejected = "refresh_rejected";
    public const string SessionRevoked = "session_revoked";
    public const string LoggedOut = "logged_out";
    public const string LoggedOutAll = "logged_out_all";
}
