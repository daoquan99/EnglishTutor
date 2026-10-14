using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyExampleEntity : Entity<Guid>
{
    public VocabularyExampleEntity() { Id = Guid.NewGuid(); }

    public Guid SenseId { get; set; }
    public string SentenceEn { get; set; } = string.Empty;
    public string? SentenceVi { get; set; }
    public string? AudioUrl { get; set; }
    public string? CefrLevel { get; set; }
    public int DisplayOrder { get; set; } = 1;
}
