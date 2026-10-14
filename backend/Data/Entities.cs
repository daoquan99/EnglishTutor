using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLiveTutor.Api.Data;

[Table("Conversations")]
public sealed class ConversationEntity
{
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];

    [MaxLength(256)]
    public string Title { get; set; } = "Buổi luyện nói mới";

    [MaxLength(16)]
    public string CefrLevel { get; set; } = "B1";

    public bool ExtendedThinking { get; set; }

    [MaxLength(64)]
    public string VoiceName { get; set; } = "Aoede";

    public double SpeechSpeed { get; set; } = 1.0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<ChatMessageEntity> Messages { get; set; } = new();
    public List<LanguageCorrectionEntity> Corrections { get; set; } = new();
}

[Table("Messages")]
public sealed class ChatMessageEntity
{
    [Key]
    [MaxLength(128)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [MaxLength(64)]
    public string ConversationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string TurnId { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string Speaker { get; set; } = "user";

    public string TranscriptLine1 { get; set; } = string.Empty;
    public string TranslationLine2 { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public int SequenceOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("Corrections")]
public sealed class LanguageCorrectionEntity
{
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];

    [Required]
    [MaxLength(64)]
    public string ConversationId { get; set; } = string.Empty;

    [MaxLength(64)]
    public string TurnId { get; set; } = string.Empty;

    public string OriginalText { get; set; } = string.Empty;
    public string CorrectedText { get; set; } = string.Empty;

    [MaxLength(64)]
    public string ErrorCategory { get; set; } = "Grammar";

    public string ExplanationVi { get; set; } = string.Empty;
    public string CefrTip { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
