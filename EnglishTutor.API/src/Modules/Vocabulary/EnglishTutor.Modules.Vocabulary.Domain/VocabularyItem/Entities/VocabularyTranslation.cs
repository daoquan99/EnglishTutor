using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyTranslation : Entity<Guid>
{
    public Guid VocabularyItemId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Meaning { get; private set; } = string.Empty;

    private VocabularyTranslation() { }

    internal static VocabularyTranslation Create(Guid vocabularyItemId, LanguageCode languageCode, string meaning)
    {
        if (vocabularyItemId == Guid.Empty)
        {
            throw new DomainException("Vocabulary item id is required.");
        }

        return new VocabularyTranslation
        {
            Id = Guid.NewGuid(),
            VocabularyItemId = vocabularyItemId,
            LanguageCode = languageCode ?? throw new DomainException("Language code is required."),
            Meaning = VocabularyItem.NormalizeRequired(meaning, 1000, "Meaning")
        };
    }
}
