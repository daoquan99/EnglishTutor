using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;

public class Correction : Entity
{
    public Guid SessionFeedbackId { get; private set; }
    public string OriginalText { get; private set; }
    public string CorrectedText { get; private set; }
    public string Explanation { get; private set; }
    public string Category { get; private set; }
    public string Severity { get; private set; }

    private Correction() : base()
    {
        OriginalText = string.Empty;
        CorrectedText = string.Empty;
        Explanation = string.Empty;
        Category = string.Empty;
        Severity = string.Empty;
    }

    public Correction(
        Guid id,
        Guid sessionFeedbackId,
        string originalText,
        string correctedText,
        string explanation,
        string category,
        string severity) : base(id)
    {
        SessionFeedbackId = sessionFeedbackId;
        OriginalText = originalText ?? string.Empty;
        CorrectedText = correctedText ?? string.Empty;
        Explanation = explanation ?? string.Empty;
        Category = category ?? string.Empty;
        Severity = severity ?? string.Empty;
    }
}
