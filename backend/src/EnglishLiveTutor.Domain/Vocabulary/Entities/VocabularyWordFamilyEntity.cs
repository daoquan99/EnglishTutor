using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyWordFamilyEntity : Entity<Guid>
{
    public VocabularyWordFamilyEntity() { Id = Guid.NewGuid(); }

    public Guid VocabularyId { get; set; }
    public string RelatedWord { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = "noun";
    public Guid? RelatedVocabularyId { get; set; }
}
