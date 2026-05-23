using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Domain.Enums;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyItem : AggregateRoot<Guid>
{
    private readonly List<VocabularyTranslation> _translations = [];
    private readonly List<VocabularyExample> _examples = [];

    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public string Word { get; private set; } = string.Empty;
    public string? Phonetic { get; private set; }
    public LanguageLevel Level { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; private set; }
    public IReadOnlyCollection<VocabularyTranslation> Translations => _translations.AsReadOnly();
    public IReadOnlyCollection<VocabularyExample> Examples => _examples.AsReadOnly();

    private VocabularyItem() { }

    public static VocabularyItem Create(
        LanguageCode targetLanguageCode,
        string word,
        string? phonetic,
        LanguageLevel level,
        string topic,
        PartOfSpeech partOfSpeech,
        DateTime utcNow)
    {
        return new VocabularyItem
        {
            Id = Guid.NewGuid(),
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            Word = NormalizeRequired(word, 200, "Word"),
            Phonetic = NormalizeOptional(phonetic, 200, "Phonetic"),
            Level = level,
            Topic = NormalizeRequired(topic, 150, "Topic"),
            PartOfSpeech = partOfSpeech,
            CreatedAtUtc = utcNow
        };
    }

    public VocabularyTranslation AddTranslation(LanguageCode languageCode, string meaning)
    {
        var translation = VocabularyTranslation.Create(Id, languageCode, meaning);
        _translations.Add(translation);
        return translation;
    }

    public VocabularyExample AddExample(string sentence, LanguageLevel level)
    {
        var example = VocabularyExample.Create(Id, TargetLanguageCode, sentence, level);
        _examples.Add(example);
        return example;
    }

    internal static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptional(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
