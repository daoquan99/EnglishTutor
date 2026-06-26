using EnglishTutor.Audit.Application.Commands.RecordSecurityEvent;
using EnglishTutor.Identity.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Audit.Infrastructure.Consumers;

/// <summary>
/// Consumes durable Identity security integration events (Batch R1, H-07) and
/// records them in the Audit store via the existing
/// <see cref="RecordSecurityEventCommand"/>.
/// </summary>
/// <remarks>
/// <para>
/// Idempotency is provided by the MassTransit Entity Framework <b>inbox</b>
/// (configured per receive endpoint by
/// <see cref="IdentitySecurityEventConsumerDefinition"/> via
/// <c>UseEntityFrameworkOutbox&lt;AuditDbContext&gt;</c>). The inbox dedupes by
/// (MessageId, ConsumerId) in <c>audit.inbox_state</c> and commits the
/// dedup row in the SAME <c>AuditDbContext</c> transaction as the
/// <c>SecurityEvent</c> row, so a redelivered message never produces a
/// duplicate audit row.
/// </para>
/// <para>
/// This consumer references only <c>EnglishTutor.Identity.Contracts</c> (the
/// integration-event contract) and <c>EnglishTutor.Audit.Application</c> — never
/// Identity.Domain or Identity.Application (AuditIdentityBoundaryTests).
/// </para>
/// </remarks>
public sealed class IdentitySecurityEventConsumer : IConsumer<IdentitySecurityEventRecordedV1>
{
    private readonly ISender _sender;
    private readonly ILogger<IdentitySecurityEventConsumer> _logger;

    public IdentitySecurityEventConsumer(ISender sender, ILogger<IdentitySecurityEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IdentitySecurityEventRecordedV1> context)
    {
        var m = context.Message;

        _logger.LogInformation(
            "Recording durable Identity security event. Category={Category} EventType={EventType} UserId={UserId} SessionId={SessionId}.",
            m.CategoryCode, m.EventType, m.UserId, m.SessionId);

        await _sender.Send(
            new RecordSecurityEventCommand(
                CategoryCode: m.CategoryCode,
                SourceModule: m.SourceModule,
                SourceEventType: m.SourceEventType,
                UserId: m.UserId,
                SessionId: m.SessionId,
                RefreshTokenFamilyId: m.RefreshTokenFamilyId,
                RefreshTokenId: m.RefreshTokenId,
                ReasonCode: m.ReasonCode,
                CorrelationId: m.CorrelationId,
                CausationId: m.CausationId,
                IpAddressHash: m.IpAddressHash,
                UserAgentHash: m.UserAgentHash,
                OccurredAtUtc: m.OccurredAtUtc),
            context.CancellationToken);
    }
}
