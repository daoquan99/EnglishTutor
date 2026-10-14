using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyGrammarPatternEntity : Entity<Guid>
{
    public VocabularyGrammarPatternEntity() { Id = Guid.NewGuid(); }

    public Guid SenseId { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? Example { get; set; }
}
