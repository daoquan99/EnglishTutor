using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Conversations;

public sealed class LanguageCorrectionEntity : Entity<string>
{
    public LanguageCorrectionEntity() { Id = Guid.NewGuid().ToString("N")[..12]; }

    public string ConversationId { get; set; } = string.Empty;
    public string TurnId { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public string CorrectedText { get; set; } = string.Empty;
    public string ErrorCategory { get; set; } = "Grammar";
    public string ExplanationVi { get; set; } = string.Empty;
    public string CefrTip { get; set; } = string.Empty;

}
