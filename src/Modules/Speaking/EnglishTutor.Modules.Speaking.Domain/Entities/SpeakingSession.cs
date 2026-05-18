using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Speaking.Domain.Enums;
using EnglishTutor.Modules.Speaking.Domain.Events;
using EnglishTutor.Modules.Speaking.Domain.ValueObjects;

namespace EnglishTutor.Modules.Speaking.Domain.Entities;

public sealed class SpeakingSession : AggregateRoot<Guid>
{
    private readonly List<SpeakingTurn> _turns = [];

    public Guid UserId { get; private set; }
    public SpeakingSessionType SessionType { get; private set; }
    public string? Topic { get; private set; }
    public LanguageSnapshot LanguageSnapshot { get; private set; } = default!;
    public SpeakingSessionStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public IReadOnlyCollection<SpeakingTurn> Turns => _turns.AsReadOnly();

    private SpeakingSession() { }

    public static SpeakingSession Start(
        Guid userId,
        LanguageSnapshot languageSnapshot,
        SpeakingSessionType sessionType,
        string? topic)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        var session = new SpeakingSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LanguageSnapshot = languageSnapshot ?? throw new DomainException("Language snapshot is required."),
            SessionType = sessionType,
            Topic = SpeakingTurn.NormalizeOptional(topic, 200, "Topic"),
            Status = SpeakingSessionStatus.Active,
            StartedAtUtc = DateTime.UtcNow
        };

        session.AddDomainEvent(new SpeakingSessionStartedDomainEvent(
            userId,
            session.Id,
            sessionType.ToString(),
            languageSnapshot.TargetLanguageCode.Value,
            languageSnapshot.UserLevel.ToString()));

        return session;
    }

    public SpeakingTurn AddTurn(string userText, string? audioUrl = null)
    {
        EnsureActive();

        var turn = SpeakingTurn.Create(Id, _turns.Count + 1, userText, audioUrl);
        _turns.Add(turn);
        return turn;
    }

    public void ApplyCorrection(
        SpeakingTurn turn,
        SpeakingTurnResult result,
        IReadOnlyList<SpeakingCorrectionMistake> mistakes)
    {
        EnsureActive();

        if (turn.SpeakingSessionId != Id || !_turns.Any(existingTurn => existingTurn.Id == turn.Id))
        {
            throw new DomainException("Turn does not belong to this speaking session.");
        }

        turn.MarkCorrected();

        AddDomainEvent(new SpeakingTurnCorrectedDomainEvent(
            UserId,
            Id,
            turn.Id,
            LanguageSnapshot.TargetLanguageCode.Value,
            LanguageSnapshot.NativeLanguageCode.Value,
            LanguageSnapshot.ExplanationLanguageCode.Value,
            result.OriginalText,
            result.CorrectedText,
            result.GrammarScore,
            result.VocabularyScore,
            result.OverallScore,
            mistakes,
            result.CreatedAtUtc));
    }

    public void Complete(int totalTurns, int overallScore)
    {
        EnsureActive();

        CompletedAtUtc = DateTime.UtcNow;
        Status = SpeakingSessionStatus.Completed;

        AddDomainEvent(new SpeakingSessionCompletedDomainEvent(
            UserId,
            Id,
            LanguageSnapshot.TargetLanguageCode.Value,
            totalTurns,
            Math.Clamp(overallScore, 0, 100),
            (long)(CompletedAtUtc.Value - StartedAtUtc).TotalSeconds,
            CompletedAtUtc.Value));
    }

    public void Abandon()
    {
        EnsureActive();
        Status = SpeakingSessionStatus.Abandoned;
        CompletedAtUtc = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != SpeakingSessionStatus.Active)
        {
            throw new DomainException("Speaking session is not active.");
        }
    }
}
