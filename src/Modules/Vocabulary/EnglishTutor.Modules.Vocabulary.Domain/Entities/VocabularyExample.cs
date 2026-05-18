using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyExample : Entity<Guid>
{
    private readonly List<VocabularyExampleTranslation> _translations = [];

    public Guid VocabularyItemId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public string Sentence { get; private set; } = string.Empty;
    public LanguageLevel Level { get; private set; }
    public IReadOnlyCollection<VocabularyExampleTranslation> Translations => _translations.AsReadOnly();

    private VocabularyExample() { }

    internal static VocabularyExample Create(Guid vocabularyItemId, LanguageCode targetLanguageCode, string sentence, LanguageLevel level)
    {
        if (vocabularyItemId == Guid.Empty)
        {
            throw new DomainException("Vocabulary item id is required.");
        }

        return new VocabularyExample
        {
            Id = Guid.NewGuid(),
            VocabularyItemId = vocabularyItemId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Sentence = VocabularyItem.NormalizeRequired(sentence, 1000, "Sentence"),
            Level = level
        };
    }

    public VocabularyExampleTranslation AddTranslation(LanguageCode languageCode, string translation)
    {
        var exampleTranslation = VocabularyExampleTranslation.Create(Id, languageCode, translation);
        _translations.Add(exampleTranslation);
        return exampleTranslation;
    }
}
