using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyGenerationAuditEntity : AggregateRoot<Guid>
{
    public VocabularyGenerationAuditEntity() { Id = Guid.NewGuid(); }

    public string UserId { get; set; } = "default-learner";
    public string ModelCode { get; set; } = string.Empty;
    public string PromptVersion { get; set; } = "VOCAB_GEN_V1";
    public int RequestedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public string Status { get; set; } = "Completed";
    public string? ErrorMessage { get; set; }

}
