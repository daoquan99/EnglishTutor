using EnglishTutor.Audit.Application.Commands.RecordSecurityEvent;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Inbox;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Identity.Contracts.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Audit.Infrastructure.Consumers;

public sealed class IdentitySecurityEventConsumer
    : RabbitMqMessageHandler<IdentitySecurityEventRecordedV1>
{
    public const string ConsumerName = "audit.identity-security.v1";
    public const string QueueName = "english.audit.identity-security.v1";

    private readonly ISender _sender;
    private readonly AuditDbContext _dbContext;
    private readonly ILogger<IdentitySecurityEventConsumer> _logger;

    public IdentitySecurityEventConsumer(
        ISender sender,
        AuditDbContext dbContext,
        ILogger<IdentitySecurityEventConsumer> logger)
    {
        _sender = sender;
        _dbContext = dbContext;
        _logger = logger;
    }

    public override MessageConsumerDescriptor Descriptor { get; } = new(
        ConsumerName: ConsumerName,
        QueueName: QueueName,
        MessageType: typeof(IdentitySecurityEventRecordedV1),
        PrefetchCount: 32,
        Concurrency: 4,
        MaxAttempts: 5,
        RetryDelay: TimeSpan.FromSeconds(10));

    protected override async Task HandleAsync(
        IdentitySecurityEventRecordedV1 message,
        MessageDeliveryContext context,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.InboxMessages.AnyAsync(
                inbox => inbox.MessageId == context.MessageId &&
                         inbox.ConsumerName == ConsumerName,
                cancellationToken))
        {
            return;
        }

        _logger.LogInformation(
            "Recording durable Identity security event. Category={Category} EventType={EventType} UserId={UserId} SessionId={SessionId}.",
            message.CategoryCode,
            message.EventType,
            message.UserId,
            message.SessionId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.InboxMessages.Add(new InboxMessage(
            messageId: context.MessageId,
            consumerName: ConsumerName,
            contractName: context.ContractName,
            schemaVersion: context.SchemaVersion,
            receivedAtUtc: context.ReceivedAtUtc));
        await _sender.Send(
            new RecordSecurityEventCommand(
                CategoryCode: message.CategoryCode,
                SourceModule: message.SourceModule,
                SourceEventType: message.SourceEventType,
                UserId: message.UserId,
                SessionId: message.SessionId,
                RefreshTokenFamilyId: message.RefreshTokenFamilyId,
                RefreshTokenId: message.RefreshTokenId,
                ReasonCode: message.ReasonCode,
                CorrelationId: message.CorrelationId,
                CausationId: message.CausationId,
                IpAddressHash: message.IpAddressHash,
                UserAgentHash: message.UserAgentHash,
                OccurredAtUtc: message.OccurredAtUtc),
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
