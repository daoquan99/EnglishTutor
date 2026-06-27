using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Entities;

public class TranscriptMessage : Entity
{
    public Guid PracticeSessionId { get; private set; }
    public int SequenceNumber { get; private set; }
    public string Role { get; private set; }
    public string Content { get; private set; }

    // EF Constructor
    private TranscriptMessage() : base()
    {
        Role = string.Empty;
        Content = string.Empty;
    }

    public TranscriptMessage(
        Guid id,
        Guid practiceSessionId,
        int sequenceNumber,
        string role,
        string content) : base(id)
    {
        PracticeSessionId = practiceSessionId;
        SequenceNumber = sequenceNumber;
        Role = role ?? string.Empty;
        Content = content ?? string.Empty;
    }
}
