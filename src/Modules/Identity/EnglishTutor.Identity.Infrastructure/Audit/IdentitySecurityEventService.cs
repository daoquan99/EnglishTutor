using EnglishTutor.Audit.Contracts;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Infrastructure.Audit;

internal sealed class IdentitySecurityEventService : IIdentitySecurityEventService
{
    private readonly ISecurityEventRecorder _recorder;
    private readonly IRefreshTokenReuseContextAccessor _contextAccessor;

    public IdentitySecurityEventService(
        ISecurityEventRecorder recorder,
        IRefreshTokenReuseContextAccessor contextAccessor)
    {
        _recorder = recorder;
        _contextAccessor = contextAccessor;
    }

    public async Task TrackLoginFailedAsync(
        Guid? userId,
        string reasonCode,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var request = new RecordSecurityEventRequest(
            CategoryCode: AuditCategoryCodes.IdentityLoginFailed,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityLoginFailed,
            UserId: userId,
            SessionId: null,
            RefreshTokenFamilyId: null,
            RefreshTokenId: null,
            ReasonCode: reasonCode,
            IpAddressHash: Hash(ipAddress) ?? _contextAccessor.IpAddressHash,
            UserAgentHash: Hash(userAgent) ?? _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: DateTime.UtcNow);

        await _recorder.RecordSecurityEventAsync(request, cancellationToken);
    }

    public async Task TrackRefreshFailedAsync(
        Guid? sessionId,
        Guid? userId,
        Guid? refreshTokenFamilyId,
        Guid? refreshTokenId,
        string reasonCode,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var request = new RecordSecurityEventRequest(
            CategoryCode: AuditCategoryCodes.IdentityRefreshFailed,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshFailed,
            UserId: userId,
            SessionId: sessionId,
            RefreshTokenFamilyId: refreshTokenFamilyId,
            RefreshTokenId: refreshTokenId,
            ReasonCode: reasonCode,
            IpAddressHash: Hash(ipAddress) ?? _contextAccessor.IpAddressHash,
            UserAgentHash: _contextAccessor.UserAgentHash,
            CorrelationId: _contextAccessor.CorrelationId,
            CausationId: null,
            OccurredAtUtc: DateTime.UtcNow);

        await _recorder.RecordSecurityEventAsync(request, cancellationToken);
    }

    private static string? Hash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
