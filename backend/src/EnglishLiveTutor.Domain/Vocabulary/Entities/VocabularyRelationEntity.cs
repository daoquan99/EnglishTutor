using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyRelationEntity : Entity<Guid>
{
    public VocabularyRelationEntity() { Id = Guid.NewGuid(); }

    public Guid SenseId { get; set; }
    public string RelationType { get; set; } = "Synonym"; // Synonym | Antonym
    public string RelatedWord { get; set; } = string.Empty;
    public Guid? RelatedVocabularyId { get; set; }
    public Guid? RelatedSenseId { get; set; }
    public int DisplayOrder { get; set; } = 1;
}
