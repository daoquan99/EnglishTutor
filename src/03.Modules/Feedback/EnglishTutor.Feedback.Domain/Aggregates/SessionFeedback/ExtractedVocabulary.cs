using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;

public class ExtractedVocabulary : Entity
{
    public Guid SessionFeedbackId { get; private set; }
    public string Term { get; private set; }
    public string Meaning { get; private set; }
    public string ExampleSentence { get; private set; }
    public string Difficulty { get; private set; }
    public double Confidence { get; private set; }

    private ExtractedVocabulary() : base()
    {
        Term = string.Empty;
        Meaning = string.Empty;
        ExampleSentence = string.Empty;
        Difficulty = string.Empty;
    }

    public ExtractedVocabulary(
        Guid id,
        Guid sessionFeedbackId,
        string term,
        string meaning,
        string exampleSentence,
        string difficulty,
        double confidence) : base(id)
    {
        SessionFeedbackId = sessionFeedbackId;
        Term = term ?? string.Empty;
        Meaning = meaning ?? string.Empty;
        ExampleSentence = exampleSentence ?? string.Empty;
        Difficulty = difficulty ?? string.Empty;
        Confidence = confidence;
    }
}
