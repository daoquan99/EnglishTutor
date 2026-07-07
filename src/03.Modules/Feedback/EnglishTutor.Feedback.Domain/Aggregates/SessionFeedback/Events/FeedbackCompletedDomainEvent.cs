using System;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;

namespace EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Events;

public record FeedbackCompletedDomainEvent(
    Guid FeedbackId,
    Guid UserId,
    Guid SessionId,
    Guid LanguagePairId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    int? Score,
    string CefrLevel) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
