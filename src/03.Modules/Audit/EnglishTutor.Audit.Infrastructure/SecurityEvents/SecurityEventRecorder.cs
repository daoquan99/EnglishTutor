using EnglishTutor.Audit.Application.Commands.RecordSecurityEvent;
using EnglishTutor.Audit.Contracts;
using MediatR;

namespace EnglishTutor.Audit.Infrastructure.SecurityEvents;

// Production implementation of EnglishTutor.Audit.Contracts.ISecurityEventRecorder.
// Sends a RecordSecurityEventCommand through MediatR. The handler in
// Audit.Application writes the row through the AuditDbContext.
//
// This implementation does NOT know about Identity. It only knows about
// Audit.Contracts (the cross-module abstraction) and the Audit
// Application command pipeline. The source event type is sourced from
// AuditCategoryCodes (a constant in Audit.Contracts) so the Identity
// assembly name does not appear as a string literal in the Audit
// Infrastructure source.
internal sealed class SecurityEventRecorder : ISecurityEventRecorder
{
    private readonly ISender _sender;

    public SecurityEventRecorder(ISender sender)
    {
        _sender = sender;
    }

    public async Task RecordRefreshTokenReuseAsync(
        RecordRefreshTokenReuseRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RecordSecurityEventCommand(
            CategoryCode: AuditCategoryCodes.IdentityRefreshTokenReuseDetected,
            SourceModule: AuditCategoryCodes.SourceModuleIdentity,
            SourceEventType: AuditCategoryCodes.SourceEventTypes.IdentityRefreshTokenReuseDetected,
            UserId: request.UserId,
            SessionId: request.SessionId,
            RefreshTokenFamilyId: request.RefreshTokenFamilyId,
            RefreshTokenId: request.RefreshTokenId,
            ReasonCode: request.ReasonCode,
            CorrelationId: request.CorrelationId,
            CausationId: request.CausationId,
            IpAddressHash: request.IpAddressHash,
            UserAgentHash: request.UserAgentHash,
            OccurredAtUtc: request.OccurredAtUtc);

        await _sender.Send(command, cancellationToken);
    }

    public async Task RecordSecurityEventAsync(
        RecordSecurityEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RecordSecurityEventCommand(
            CategoryCode: request.CategoryCode,
            SourceModule: request.SourceModule,
            SourceEventType: request.SourceEventType,
            UserId: request.UserId,
            SessionId: request.SessionId,
            RefreshTokenFamilyId: request.RefreshTokenFamilyId,
            RefreshTokenId: request.RefreshTokenId,
            ReasonCode: request.ReasonCode,
            CorrelationId: request.CorrelationId,
            CausationId: request.CausationId,
            IpAddressHash: request.IpAddressHash,
            UserAgentHash: request.UserAgentHash,
            OccurredAtUtc: request.OccurredAtUtc);

        await _sender.Send(command, cancellationToken);
    }
}
