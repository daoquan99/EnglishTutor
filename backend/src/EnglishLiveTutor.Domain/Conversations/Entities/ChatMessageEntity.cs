using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Conversations;

public sealed class ChatMessageEntity : Entity<string>
{
    public ChatMessageEntity() { Id = Guid.NewGuid().ToString("N"); }

    public string ConversationId { get; set; } = string.Empty;
    public string TurnId { get; set; } = string.Empty;
    public string Speaker { get; set; } = "user";
    public string TranscriptLine1 { get; set; } = string.Empty;
    public string TranslationLine2 { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public int SequenceOrder { get; set; }

}
