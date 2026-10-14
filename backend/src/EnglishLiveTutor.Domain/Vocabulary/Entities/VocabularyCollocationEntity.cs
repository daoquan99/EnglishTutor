using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyCollocationEntity : Entity<Guid>
{
    public VocabularyCollocationEntity() { Id = Guid.NewGuid(); }

    public Guid SenseId { get; set; }
    public string PhraseEn { get; set; } = string.Empty;
    public string? MeaningVi { get; set; }
    public int DisplayOrder { get; set; } = 1;
}
