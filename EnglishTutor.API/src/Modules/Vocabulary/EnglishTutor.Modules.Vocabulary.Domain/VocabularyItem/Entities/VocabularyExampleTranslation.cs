using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyExampleTranslation : Entity<Guid>
{
    public Guid VocabularyExampleId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Translation { get; private set; } = string.Empty;

    private VocabularyExampleTranslation() { }

    internal static VocabularyExampleTranslation Create(Guid vocabularyExampleId, LanguageCode languageCode, string translation)
    {
        if (vocabularyExampleId == Guid.Empty)
        {
            throw new DomainException("Vocabulary example id is required.");
        }

        return new VocabularyExampleTranslation
        {
            Id = Guid.NewGuid(),
            VocabularyExampleId = vocabularyExampleId,
            LanguageCode = languageCode ?? throw new DomainException("Language code is required."),
            Translation = VocabularyItem.NormalizeRequired(translation, 1000, "Translation")
        };
    }
}
