namespace EnglishTutor.Audit.Contracts;

// String constants for SecurityEvent.CategoryCode and SourceEventType.
// Stored as string columns in audit.security_events. NOT an enum:
// operational data that may expand over time, and string codes are
// clearer in DB queries and easier to evolve than enum ordinals.
public static class AuditCategoryCodes
{
    // ---- CategoryCode values ----

    public const string IdentityLoginSucceeded = "identity.login_succeeded";
    public const string IdentityLoginFailed = "identity.login_failed";
    public const string IdentityRefreshSucceeded = "identity.refresh_succeeded";
    public const string IdentityRefreshFailed = "identity.refresh_failed";
    public const string IdentityRefreshTokenReuseDetected = "identity.refresh_token_reuse_detected";
    public const string IdentityLogoutSucceeded = "identity.logout_succeeded";
    public const string IdentityLogoutAllSucceeded = "identity.logout_all_succeeded";
    public const string IdentitySessionRevoked = "identity.session_revoked";

    // ---- SourceModule values ----

    // Source module for events produced by the Identity module.
    public const string SourceModuleIdentity = "identity";

    // ---- SourceEventType values ----

    // Fully-qualified CLR type names of the source domain events. Lives
    // here so the Audit.Infrastructure source never hardcodes an
    // Identity assembly-name string literal — every consumer of the
    // SecurityEvent.SourceEventType column reads the value from here.
    public static class SourceEventTypes
    {
        public const string IdentityLoginSucceeded =
            "EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.UserSessionCreatedDomainEvent";
        public const string IdentityLoginFailed =
            "EnglishTutor.Identity.Application.Commands.Login.LoginCommand";
        public const string IdentityRefreshSucceeded =
            "EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.RefreshTokenRotatedDomainEvent";
        public const string IdentityRefreshFailed =
            "EnglishTutor.Identity.Application.Commands.Refresh.RefreshCommand";
        public const string IdentityRefreshTokenReuseDetected =
            "EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.RefreshTokenReuseDetectedDomainEvent";
        public const string IdentityLogoutSucceeded =
            "EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.UserSessionRevokedDomainEvent";
        public const string IdentityLogoutAllSucceeded =
            "EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.UserSessionRevokedDomainEvent";
        public const string IdentitySessionRevoked =
            "EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.UserSessionRevokedDomainEvent";
    }
}
