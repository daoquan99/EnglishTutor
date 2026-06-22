using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.Audit.Contracts;

namespace EnglishTutor.Audit.Application.SecurityEvents;

// Internal command used by Audit.Infrastructure.SecurityEventRecorder to
// hand off a security-event record to the Audit write pipeline. Carries
// the same payload as the public RecordRefreshTokenReuseRequest plus
// SourceEventType (CLR type name) and a default DateTimeProvider stamp.
public sealed record RecordSecurityEventCommand(
    string CategoryCode,
    string SourceModule,
    string SourceEventType,
    Guid? UserId,
    Guid? SessionId,
    Guid? RefreshTokenFamilyId,
    Guid? RefreshTokenId,
    string? ReasonCode,
    Guid? CorrelationId,
    Guid? CausationId,
    string? IpAddressHash,
    string? UserAgentHash,
    DateTime OccurredAtUtc) : ICommand;
