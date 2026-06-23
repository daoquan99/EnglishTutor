namespace EnglishTutor.Audit.Contracts;

// String constants for SecurityEvent.CategoryCode and SourceEventType.
// Stored as string columns in audit.security_events. NOT an enum:
// operational data that may expand over time, and string codes are
// clearer in DB queries and easier to evolve than enum ordinals.
public static class AuditCategoryCodes
{
    // ---- CategoryCode values ----

    // Refresh-token reuse detected. Identity's
    // RefreshTokenReuseDetectedDomainEvent maps to this category. This is
    // the ONLY category that Task 22A records. Future tasks add more
    // categories (login success / failure, logout, etc.).
    public const string IdentityRefreshTokenReuseDetected = "identity.refresh_token_reuse_detected";

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
        public const string IdentityRefreshTokenReuseDetected =
            "EnglishTutor.Identity.Domain.Aggregates.Sessions.Events.RefreshTokenReuseDetectedDomainEvent";
    }
}
