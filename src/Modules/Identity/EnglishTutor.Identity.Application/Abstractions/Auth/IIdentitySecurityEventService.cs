using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Application.Abstractions.Auth;

/// <summary>
/// Domain-neutral application abstraction for tracking failed identity security events.
/// This prevents direct project reference from Identity.Application to Audit.Contracts,
/// preserving architectural boundaries.
/// </summary>
public interface IIdentitySecurityEventService
{
    Task TrackLoginFailedAsync(
        Guid? userId,
        string reasonCode,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task TrackRefreshFailedAsync(
        Guid? sessionId,
        Guid? userId,
        Guid? refreshTokenFamilyId,
        Guid? refreshTokenId,
        string reasonCode,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
