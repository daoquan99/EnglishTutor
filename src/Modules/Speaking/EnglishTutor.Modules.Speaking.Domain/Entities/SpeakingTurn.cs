using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Speaking.Domain.Enums;

namespace EnglishTutor.Modules.Speaking.Domain.Entities;

public sealed class SpeakingTurn : Entity<Guid>
{
    public Guid SpeakingSessionId { get; private set; }
    public int TurnNumber { get; private set; }
    public string UserText { get; private set; } = string.Empty;
    public string? AudioUrl { get; private set; }
    public SpeakingTurnStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private SpeakingTurn() { }

    internal static SpeakingTurn Create(Guid speakingSessionId, int turnNumber, string userText, string? audioUrl)
    {
        if (speakingSessionId == Guid.Empty)
        {
            throw new DomainException("Speaking session id is required.");
        }

        if (turnNumber <= 0)
        {
            throw new DomainException("Turn number must be positive.");
        }

        return new SpeakingTurn
        {
            Id = Guid.NewGuid(),
            SpeakingSessionId = speakingSessionId,
            TurnNumber = turnNumber,
            UserText = NormalizeRequired(userText, 4000, "User text"),
            AudioUrl = NormalizeOptional(audioUrl, 2048, "Audio url"),
            Status = SpeakingTurnStatus.PendingCorrection,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkCorrected() => Status = SpeakingTurnStatus.Corrected;

    public void MarkFailed() => Status = SpeakingTurnStatus.Failed;

    internal static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptional(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
