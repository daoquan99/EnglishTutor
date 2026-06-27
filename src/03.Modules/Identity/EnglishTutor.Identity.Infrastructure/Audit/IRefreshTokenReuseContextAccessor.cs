namespace EnglishTutor.Identity.Infrastructure.Audit;

// Per-request accessor for the IP / User-Agent context captured at the API
// boundary. Read by RefreshTokenReuseAuditHandler when the domain event
// fires. Returns null outside an HTTP request (e.g. background job tests).
public interface IRefreshTokenReuseContextAccessor
{
    string? IpAddressHash { get; }
    string? UserAgentHash { get; }
    Guid? CorrelationId { get; }
}
