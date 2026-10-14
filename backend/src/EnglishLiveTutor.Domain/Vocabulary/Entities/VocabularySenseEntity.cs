using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularySenseEntity : Entity<Guid>
{
    public VocabularySenseEntity() { Id = Guid.NewGuid(); }

    public Guid VocabularyId { get; set; }
    public string PartOfSpeech { get; set; } = "noun";
    public string DefinitionEn { get; set; } = string.Empty;
    public string MeaningVi { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string CefrLevel { get; set; } = "B1"; // A1, A2, B1, B2, C1, C2
    public bool IsPrimary { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;

    public List<VocabularyRelationEntity> Relations { get; set; } = new();
    public List<VocabularyCollocationEntity> Collocations { get; set; } = new();
    public List<VocabularyExampleEntity> Examples { get; set; } = new();
    public List<VocabularyGrammarPatternEntity> GrammarPatterns { get; set; } = new();
}
