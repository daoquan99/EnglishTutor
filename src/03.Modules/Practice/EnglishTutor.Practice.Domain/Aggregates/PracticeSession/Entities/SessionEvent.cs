using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Entities;

public class SessionEvent : Entity
{
    public Guid PracticeSessionId { get; private set; }
    public string EventType { get; private set; }
    public string? Payload { get; private set; }

    // EF Constructor
    private SessionEvent() : base()
    {
        EventType = string.Empty;
    }

    public SessionEvent(
        Guid id,
        Guid practiceSessionId,
        string eventType,
        string? payload) : base(id)
    {
        PracticeSessionId = practiceSessionId;
        EventType = eventType ?? string.Empty;
        Payload = payload;
    }
}
