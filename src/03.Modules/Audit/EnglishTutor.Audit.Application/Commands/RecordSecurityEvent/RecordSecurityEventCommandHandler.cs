using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Audit.Application.Abstractions.Persistence;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.Repositories;

namespace EnglishTutor.Audit.Application.Commands.RecordSecurityEvent;

// Handles RecordSecurityEventCommand: creates a SecurityEvent aggregate
// and persists it through the Audit UoW. Idempotent: re-running the
// command with the same payload creates a new row (Audit is append-only;
// we do not deduplicate on purpose, because each row has its own
// correlation id and is independently useful for forensic timelines).
public sealed class RecordSecurityEventCommandHandler : ICommandHandler<RecordSecurityEventCommand>
{
    private readonly ISecurityEventRepository _repository;
    private readonly IAuditUnitOfWork _unitOfWork;

    public RecordSecurityEventCommandHandler(
        ISecurityEventRepository repository,
        IAuditUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RecordSecurityEventCommand request,
        CancellationToken cancellationToken)
    {
        var securityEvent = SecurityEvent.Create(
            categoryCode: request.CategoryCode,
            sourceModule: request.SourceModule,
            sourceEventType: request.SourceEventType,
            userId: request.UserId,
            sessionId: request.SessionId,
            refreshTokenFamilyId: request.RefreshTokenFamilyId,
            refreshTokenId: request.RefreshTokenId,
            reasonCode: request.ReasonCode,
            correlationId: request.CorrelationId,
            causationId: request.CausationId,
            ipAddressHash: request.IpAddressHash,
            userAgentHash: request.UserAgentHash,
            occurredAtUtc: request.OccurredAtUtc);

        await _repository.AddAsync(securityEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
