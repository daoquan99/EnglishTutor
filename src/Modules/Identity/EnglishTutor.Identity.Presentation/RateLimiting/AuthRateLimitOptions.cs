namespace EnglishTutor.Identity.Presentation.RateLimiting;

/// <summary>
/// Options-driven limits for the auth rate-limiter policies. Defaults are the
/// Phase 1 design starting values; they are configurable under
/// <c>Auth:RateLimit</c>. The R1 limiter is in-process (no Redis); a
/// distributed limiter is a separate, out-of-scope change (Batch R1, H-06).
/// </summary>
public sealed class AuthRateLimitOptions
{
    public const string SectionName = "Auth:RateLimit";

    /// <summary>When false, all policies become no-op limiters. Used by tests
    /// that exercise unrelated flows from a single client IP.</summary>
    public bool Enabled { get; init; } = true;

    public PolicyLimit Login { get; init; } = new() { PermitLimit = 10, WindowSeconds = 300 };
    public PolicyLimit Refresh { get; init; } = new() { PermitLimit = 30, WindowSeconds = 300 };
    public PolicyLimit Logout { get; init; } = new() { PermitLimit = 30, WindowSeconds = 300 };
    public PolicyLimit SessionMutation { get; init; } = new() { PermitLimit = 30, WindowSeconds = 300 };

    public sealed class PolicyLimit
    {
        public int PermitLimit { get; init; }
        public int WindowSeconds { get; init; }
    }
}

/// <summary>Stable named rate-limiter policy identifiers applied to endpoints.</summary>
public static class AuthRateLimitPolicies
{
    public const string Login = "auth-login";
    public const string Refresh = "auth-refresh";
    public const string Logout = "auth-logout";
    public const string SessionMutation = "auth-session-mutation";
}
