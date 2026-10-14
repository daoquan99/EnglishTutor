namespace EnglishLiveTutor.Domain.Conversations;

public sealed class ConversationEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string Title { get; set; } = "Buổi luyện nói mới";
    public string CefrLevel { get; set; } = "B1";
    public bool ExtendedThinking { get; set; }
    public string VoiceName { get; set; } = "Aoede";
    public double SpeechSpeed { get; set; } = 1.0;
    public string ConversationMode { get; set; } = "daily_conversation";
    public string Topic { get; set; } = string.Empty;
    public string? CustomSystemPrompt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

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

public sealed class ChatMessageEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ConversationId { get; set; } = string.Empty;
    public string TurnId { get; set; } = string.Empty;
    public string Speaker { get; set; } = "user";
    public string TranscriptLine1 { get; set; } = string.Empty;
    public string TranslationLine2 { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public int SequenceOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class LanguageCorrectionEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string ConversationId { get; set; } = string.Empty;
    public string TurnId { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public string CorrectedText { get; set; } = string.Empty;
    public string ErrorCategory { get; set; } = "Grammar";
    public string ExplanationVi { get; set; } = string.Empty;
    public string CefrTip { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
