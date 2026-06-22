namespace EnglishTutor.Audit.Contracts;

// Cross-module contract for recording security events. Implemented by
// Audit.Infrastructure.SecurityEventRecorder. Identity.Infrastructure is
// the only known caller in this slice; future modules may use the same
// contract.
//
// This interface is the SOLE boundary between the Identity module and the
// Audit module. Identity does not reference Audit.Domain, Audit.Application,
// Audit.Infrastructure, or Audit.Presentation. The only Audit assembly
// Identity may reference is EnglishTutor.Audit.Contracts.
//
// Implementations MUST NOT log the raw refresh token (it is not in scope of
// the request) and MUST NOT persist the refresh-token hash (Audit records
// the refresh-token id, not the hash). If a future method on this
// interface needs the hash, the request DTO must explicitly require it.
public interface ISecurityEventRecorder
{
    Task RecordRefreshTokenReuseAsync(
        RecordRefreshTokenReuseRequest request,
        CancellationToken cancellationToken = default);
}
