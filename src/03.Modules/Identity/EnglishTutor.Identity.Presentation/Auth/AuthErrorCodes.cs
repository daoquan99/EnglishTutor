namespace EnglishTutor.Identity.Presentation.Auth;

/// <summary>
/// Stable application error codes returned in the <c>errorCode</c> extension of
/// RFC ProblemDetails responses for auth endpoints. These are part of the API
/// contract: clients branch on these codes, not on localized messages
/// (Batch R1 / <c>api-contracts.md</c>). Codes never reveal whether an account
/// or token previously existed.
/// </summary>
public static class AuthErrorCodes
{
    public const string RefreshInvalid = "auth.refresh.invalid";
    public const string RefreshCookieMissing = "auth.refresh.cookie_missing";
    public const string CsrfMissing = "auth.csrf.missing";
    public const string CsrfInvalid = "auth.csrf.invalid";
    public const string RateLimited = "auth.rate_limited";
    public const string Unauthorized = "auth.unauthorized";
}
