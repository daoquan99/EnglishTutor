using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Conversations;

public sealed class ConversationEntity : AggregateRoot<string>
{
    public ConversationEntity() { Id = Guid.NewGuid().ToString("N")[..12]; }

    public string Title { get; set; } = "Buổi luyện nói mới";
    public string CefrLevel { get; set; } = "B1";
    public bool ExtendedThinking { get; set; }
    public string VoiceName { get; set; } = "Aoede";
    public double SpeechSpeed { get; set; } = 1.0;
    public string ConversationMode { get; set; } = "daily_conversation";
    public string Topic { get; set; } = string.Empty;
    public string? CustomSystemPrompt { get; set; }

    public List<ChatMessageEntity> Messages { get; set; } = new();
    public List<LanguageCorrectionEntity> Corrections { get; set; } = new();

    public void UpdateSettings(
        string? title,
        string cefrLevel,
        bool extendedThinking,
        string voiceName,
        double speechSpeed,
        string conversationMode,
        string topic,
        string? customSystemPrompt)
    {
        CefrLevel = cefrLevel;
        ExtendedThinking = extendedThinking;
        VoiceName = voiceName;
        SpeechSpeed = speechSpeed;
        ConversationMode = string.IsNullOrWhiteSpace(conversationMode) ? "daily_conversation" : conversationMode.Trim();
        Topic = (topic ?? string.Empty).Trim();
        CustomSystemPrompt = string.IsNullOrWhiteSpace(customSystemPrompt) ? null : customSystemPrompt.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }
        else if (!string.IsNullOrWhiteSpace(Topic) && Title.StartsWith("Hội thoại ", StringComparison.OrdinalIgnoreCase))
        {
            Title = $"{Topic} ({cefrLevel})";
        }
    }

    public void UpdateTitleFromFirstUserMessage(string fullTranscript)
    {
        if (string.IsNullOrWhiteSpace(Topic) &&
            Title.StartsWith("Hội thoại ", StringComparison.OrdinalIgnoreCase) &&
            fullTranscript.Length > 6)
        {
            Title = fullTranscript.Length > 42 ? fullTranscript[..42] + "..." : fullTranscript;
        }
    }
}
