using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;

public class MistakePattern : Entity
{
    public Guid SessionFeedbackId { get; private set; }
    public string Pattern { get; private set; }
    public string Description { get; private set; }
    public int Frequency { get; private set; }

    private MistakePattern() : base()
    {
        Pattern = string.Empty;
        Description = string.Empty;
    }

    public MistakePattern(
        Guid id,
        Guid sessionFeedbackId,
        string pattern,
        string description,
        int frequency) : base(id)
    {
        SessionFeedbackId = sessionFeedbackId;
        Pattern = pattern ?? string.Empty;
        Description = description ?? string.Empty;
        Frequency = frequency;
    }
}
