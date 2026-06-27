using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Application.Abstractions.Auth;

public interface IIdentitySecurityEventPublisher
{
    Task PublishAsync(IdentitySecurityEventData data, CancellationToken cancellationToken = default);
}

public sealed record IdentitySecurityEventData(
    string EventType,
    string CategoryCode,
    string SourceEventType,
    Guid? UserId,
    Guid? SessionId,
    Guid? RefreshTokenFamilyId,
    Guid? RefreshTokenId,
    string? ReasonCode,
    string? IpAddressHash,
    string? UserAgentHash,
    Guid? CorrelationId,
    Guid? CausationId,
    DateTime OccurredAtUtc);
