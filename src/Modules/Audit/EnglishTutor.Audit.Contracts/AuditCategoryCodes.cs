namespace EnglishTutor.Audit.Contracts;

// String constants for SecurityEvent.CategoryCode. Stored as a string column
// in audit.security_events. NOT an enum: operational data that may expand
// over time, and string codes are clearer in DB queries and easier to evolve
// than enum ordinals.
public static class AuditCategoryCodes
{
    // Refresh-token reuse detected. Identity's
    // RefreshTokenReuseDetectedDomainEvent maps to this category. This is
    // the ONLY category that Task 22A records. Future tasks add more
    // categories (login success / failure, logout, etc.).
    public const string IdentityRefreshTokenReuseDetected = "identity.refresh_token_reuse_detected";

    // Source module for events produced by the Identity module.
    public const string SourceModuleIdentity = "identity";
}
