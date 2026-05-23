using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Speaking.Domain.Entities;

public sealed class ConversationPracticeResult : Entity<Guid>
{
    public Guid SpeakingSessionId { get; private set; }
    public Guid ConversationScenarioId { get; private set; }
    public int TaskCompletionScore { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
    private ConversationPracticeResult() { }

    public static ConversationPracticeResult Create(
        Guid speakingSessionId,
        Guid conversationScenarioId,
        int taskCompletionScore,
        string feedback,
        DateTime utcNow)
    {
        if (speakingSessionId == Guid.Empty || conversationScenarioId == Guid.Empty)
        {
            throw new DomainException("Speaking session id and conversation scenario id are required.");
        }

        return new ConversationPracticeResult
        {
            Id = Guid.NewGuid(),
            SpeakingSessionId = speakingSessionId,
            ConversationScenarioId = conversationScenarioId,
            TaskCompletionScore = Math.Clamp(taskCompletionScore, 0, 100),
            Feedback = SpeakingTurn.NormalizeRequired(feedback, 4000, "Feedback"),
            CreatedAtUtc = utcNow
        };
    }
}
