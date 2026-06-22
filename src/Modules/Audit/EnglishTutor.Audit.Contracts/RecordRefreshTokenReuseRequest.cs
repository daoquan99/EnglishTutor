namespace EnglishTutor.Audit.Contracts;

// Cross-module request to record a refresh-token-reuse-detected security
// event. Contains ONLY ids, reason code, and hashed context. NEVER the raw
// refresh token. NEVER the refresh-token hash. The Identity module sends
// this to the Audit module through ISecurityEventRecorder.
//
// Hashes (IpAddressHash, UserAgentHash) are SHA-256 of the original
// values, computed at the API boundary. Audit.Infrastructure does NOT see
// the plaintext; the Identity infrastructure handler passes the hashes
// in directly.
public sealed record RecordRefreshTokenReuseRequest(
    Guid UserId,
    Guid SessionId,
    Guid RefreshTokenFamilyId,
    Guid RefreshTokenId,
    string ReasonCode,
    string? IpAddressHash,
    string? UserAgentHash,
    Guid? CorrelationId,
    Guid? CausationId,
    DateTime OccurredAtUtc);
