using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Domain.Shared;

namespace EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;

/// <summary>
/// Aggregate root representing a vocabulary item under a specific topic.
/// </summary>
public sealed class TopicVocabulary : AggregateRoot
{
    public Guid TopicId { get; private set; }
    public string Word { get; private set; } = null!;
    public string WordNormalized { get; private set; } = null!;
    public string Definition { get; private set; } = null!;
    public string? PartOfSpeech { get; private set; }
    public string? Phonetic { get; private set; }
    public string? ExampleSentence { get; private set; }
    public string? ExampleTranslation { get; private set; }
    public bool IsActive { get; private set; }

    private TopicVocabulary()
    {
    }

    public static TopicVocabulary Create(
        Guid topicId,
        string word,
        string definition,
        string? partOfSpeech,
        string? phonetic,
        string? exampleSentence,
        string? exampleTranslation)
    {
        if (topicId == Guid.Empty)
        {
            throw new ArgumentException("TopicId cannot be empty.", nameof(topicId));
        }

        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ArgumentException("Word cannot be empty.", nameof(word));
        }

        if (string.IsNullOrWhiteSpace(definition))
        {
            throw new ArgumentException("Definition cannot be empty.", nameof(definition));
        }

        return new TopicVocabulary
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            Word = word.Trim(),
            WordNormalized = TextNormalizer.Normalize(word),
            Definition = definition.Trim(),
            PartOfSpeech = partOfSpeech?.Trim(),
            Phonetic = phonetic?.Trim(),
            ExampleSentence = exampleSentence?.Trim(),
            ExampleTranslation = exampleTranslation?.Trim(),
            IsActive = true
        };
    }

    public void Update(
        string word,
        string definition,
        string? partOfSpeech,
        string? phonetic,
        string? exampleSentence,
        string? exampleTranslation,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ArgumentException("Word cannot be empty.", nameof(word));
        }

        if (string.IsNullOrWhiteSpace(definition))
        {
            throw new ArgumentException("Definition cannot be empty.", nameof(definition));
        }

        Word = word.Trim();
        WordNormalized = TextNormalizer.Normalize(word);
        Definition = definition.Trim();
        PartOfSpeech = partOfSpeech?.Trim();
        Phonetic = phonetic?.Trim();
        ExampleSentence = exampleSentence?.Trim();
        ExampleTranslation = exampleTranslation?.Trim();
        IsActive = isActive;
    }

    public void Enable()
    {
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }
}
