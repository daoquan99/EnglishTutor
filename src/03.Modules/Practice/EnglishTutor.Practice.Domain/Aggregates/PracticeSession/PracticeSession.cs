using System;
using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Entities;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Events;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession;

public class PracticeSession : AggregateRoot
{
    private readonly List<TranscriptMessage> _transcriptMessages = [];
    private readonly List<SessionEvent> _sessionEvents = [];

    public Guid UserId { get; private set; }
    public Guid QuotaReservationId { get; private set; }
    public Guid RouteLeaseId { get; private set; }
    public PracticeSessionStatus Status { get; private set; }
    public PracticeSessionEndReason? EndReason { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public PracticeSessionScenarioSnapshot ScenarioSnapshot { get; private set; }

    public IReadOnlyCollection<TranscriptMessage> TranscriptMessages => _transcriptMessages.AsReadOnly();
    public IReadOnlyCollection<SessionEvent> SessionEvents => _sessionEvents.AsReadOnly();

    // EF Constructor
    private PracticeSession() : base()
    {
        ScenarioSnapshot = null!;
    }

    public PracticeSession(
        Guid id,
        Guid userId,
        Guid quotaReservationId,
        Guid routeLeaseId,
        PracticeSessionScenarioSnapshot scenarioSnapshot,
        DateTime startedAtUtc,
        TimeSpan sessionDuration) : base(id)
    {
        UserId = userId;
        QuotaReservationId = quotaReservationId;
        RouteLeaseId = routeLeaseId;
        ScenarioSnapshot = scenarioSnapshot ?? throw new ArgumentNullException(nameof(scenarioSnapshot));
        Status = PracticeSessionStatus.Active;
        StartedAtUtc = startedAtUtc;
        ExpiresAtUtc = startedAtUtc.Add(sessionDuration);

        // Add started event
        AddSessionEvent("SessionStarted", $"Session started for user {userId} with scenario {scenarioSnapshot.ScenarioId}.");

        // Raise domain event
        RaiseDomainEvent(new PracticeSessionStartedDomainEvent(
            Id,
            UserId,
            ScenarioSnapshot.ScenarioId,
            ScenarioSnapshot.ModeDefinitionId,
            QuotaReservationId,
            RouteLeaseId));
    }

    public void AppendMessage(Guid messageId, string role, string content, DateTime timestamp)
    {
        EnsureActive(timestamp);

        int nextSequence = _transcriptMessages.Count + 1;
        var message = new TranscriptMessage(messageId, Id, nextSequence, role, content);
        _transcriptMessages.Add(message);

        AddSessionEvent("MessageAppended", $"Message appended by {role}. MessageId: {messageId}.");
    }

    public void CompleteByUser(DateTime endedAtUtc)
    {
        if (Status != PracticeSessionStatus.Active)
        {
            return;
        }

        Status = PracticeSessionStatus.Completed;
        EndReason = PracticeSessionEndReason.UserEnded;
        EndedAtUtc = endedAtUtc;

        AddSessionEvent("SessionEnded", "Session ended by user.");

        int durationSeconds = (int)(EndedAtUtc.Value - StartedAtUtc).TotalSeconds;

        RaiseDomainEvent(new PracticeSessionEndedDomainEvent(
            Id,
            UserId,
            QuotaReservationId,
            RouteLeaseId,
            Status,
            durationSeconds,
            EndReason.Value.ToString()));
    }

    public void CancelByUser(DateTime endedAtUtc)
    {
        if (Status != PracticeSessionStatus.Active)
        {
            return;
        }

        Status = PracticeSessionStatus.Cancelled;
        EndReason = PracticeSessionEndReason.UserCancelled;
        EndedAtUtc = endedAtUtc;

        AddSessionEvent("SessionCancelled", "Session cancelled by user.");

        int durationSeconds = (int)(EndedAtUtc.Value - StartedAtUtc).TotalSeconds;

        RaiseDomainEvent(new PracticeSessionEndedDomainEvent(
            Id,
            UserId,
            QuotaReservationId,
            RouteLeaseId,
            Status,
            durationSeconds,
            EndReason.Value.ToString()));
    }

    public void Expire(DateTime expiredAtUtc)
    {
        if (Status != PracticeSessionStatus.Active)
        {
            return;
        }

        Status = PracticeSessionStatus.Expired;
        EndReason = PracticeSessionEndReason.Expired;
        EndedAtUtc = expiredAtUtc;

        AddSessionEvent("SessionExpired", "Session expired due to inactivity.");

        int durationSeconds = (int)(EndedAtUtc.Value - StartedAtUtc).TotalSeconds;

        RaiseDomainEvent(new PracticeSessionEndedDomainEvent(
            Id,
            UserId,
            QuotaReservationId,
            RouteLeaseId,
            Status,
            durationSeconds,
            EndReason.Value.ToString()));
    }

    public void MarkFailed(DateTime failedAtUtc, string reasonCode)
    {
        if (Status != PracticeSessionStatus.Active)
        {
            return;
        }

        Status = PracticeSessionStatus.Failed;
        EndReason = PracticeSessionEndReason.SystemFailed;
        EndedAtUtc = failedAtUtc;

        AddSessionEvent("SessionFailed", $"Session failed. Reason: {reasonCode}");

        int durationSeconds = (int)(EndedAtUtc.Value - StartedAtUtc).TotalSeconds;

        RaiseDomainEvent(new PracticeSessionEndedDomainEvent(
            Id,
            UserId,
            QuotaReservationId,
            RouteLeaseId,
            Status,
            durationSeconds,
            EndReason.Value.ToString()));
    }

    public void CompleteScenario(DateTime completedAtUtc)
    {
        if (Status != PracticeSessionStatus.Active)
        {
            return;
        }

        Status = PracticeSessionStatus.Completed;
        EndReason = PracticeSessionEndReason.ScenarioCompleted;
        EndedAtUtc = completedAtUtc;

        AddSessionEvent("ScenarioCompleted", "Session completed because the scenario was completed.");

        int durationSeconds = (int)(EndedAtUtc.Value - StartedAtUtc).TotalSeconds;

        RaiseDomainEvent(new PracticeSessionEndedDomainEvent(
            Id,
            UserId,
            QuotaReservationId,
            RouteLeaseId,
            Status,
            durationSeconds,
            EndReason.Value.ToString()));
    }

    public bool CheckExpiry(DateTime currentUtc)
    {
        if (Status == PracticeSessionStatus.Active && currentUtc > ExpiresAtUtc)
        {
            Expire(currentUtc);
            return true;
        }
        return false;
    }

    private void EnsureActive(DateTime currentUtc)
    {
        if (CheckExpiry(currentUtc))
        {
            throw new InvalidOperationException("Cannot perform this action because the session has expired.");
        }

        if (Status != PracticeSessionStatus.Active)
        {
            throw new InvalidOperationException($"Cannot perform this action because the session status is {Status}.");
        }
    }

    private void AddSessionEvent(string eventType, string? payload)
    {
        var sessionEvent = new SessionEvent(Guid.NewGuid(), Id, eventType, payload);
        _sessionEvents.Add(sessionEvent);
    }
}
