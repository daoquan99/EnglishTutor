using System;
using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;

public class SessionFeedback : AggregateRoot
{
    private readonly List<Correction> _corrections = new();
    private readonly List<ExtractedVocabulary> _vocabulary = new();
    private readonly List<MistakePattern> _mistakePatterns = new();

    public Guid PracticeSessionId { get; private set; }
    public Guid UserId { get; private set; }
    public string Summary { get; private set; }
    public string Strengths { get; private set; }
    public string ImprovementAreas { get; private set; }
    public int? Score { get; private set; }
    public string CefrLevel { get; private set; }
    public FeedbackStatus Status { get; private set; }
    public string FailureReasonCode { get; private set; }

    public IReadOnlyCollection<Correction> Corrections => _corrections.AsReadOnly();
    public IReadOnlyCollection<ExtractedVocabulary> Vocabulary => _vocabulary.AsReadOnly();
    public IReadOnlyCollection<MistakePattern> MistakePatterns => _mistakePatterns.AsReadOnly();

    private SessionFeedback() : base()
    {
        Summary = string.Empty;
        Strengths = string.Empty;
        ImprovementAreas = string.Empty;
        CefrLevel = string.Empty;
        FailureReasonCode = string.Empty;
    }

    public static SessionFeedback CreatePending(
        Guid id,
        Guid practiceSessionId,
        Guid userId)
    {
        if (practiceSessionId == Guid.Empty)
            throw new ArgumentException("Practice session ID is required.", nameof(practiceSessionId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        return new SessionFeedback
        {
            Id = id,
            PracticeSessionId = practiceSessionId,
            UserId = userId,
            Summary = string.Empty,
            Strengths = string.Empty,
            ImprovementAreas = string.Empty,
            CefrLevel = string.Empty,
            Status = FeedbackStatus.Pending,
            FailureReasonCode = string.Empty
        };
    }

    public void CompleteSuccess(
        string summary,
        string strengths,
        string improvementAreas,
        int? score,
        string cefrLevel,
        List<Correction> corrections,
        List<ExtractedVocabulary> vocabulary,
        List<MistakePattern> mistakePatterns)
    {
        if (Status != FeedbackStatus.Pending)
            throw new InvalidOperationException($"Cannot complete feedback in {Status} state.");

        Summary = summary ?? string.Empty;
        Strengths = strengths ?? string.Empty;
        ImprovementAreas = improvementAreas ?? string.Empty;
        Score = score;
        CefrLevel = cefrLevel ?? string.Empty;
        Status = FeedbackStatus.Success;

        _corrections.Clear();
        if (corrections != null)
        {
            _corrections.AddRange(corrections);
        }

        _vocabulary.Clear();
        if (vocabulary != null)
        {
            _vocabulary.AddRange(vocabulary);
        }

        _mistakePatterns.Clear();
        if (mistakePatterns != null)
        {
            _mistakePatterns.AddRange(mistakePatterns);
        }

        RaiseDomainEvent(new Events.FeedbackCompletedDomainEvent(Id, UserId, PracticeSessionId));
    }

    public void CompleteFailed(string failureReasonCode)
    {
        if (Status != FeedbackStatus.Pending)
            throw new InvalidOperationException($"Cannot fail feedback in {Status} state.");

        Status = FeedbackStatus.Failed;
        FailureReasonCode = failureReasonCode ?? "unknown_failure";
    }
}
