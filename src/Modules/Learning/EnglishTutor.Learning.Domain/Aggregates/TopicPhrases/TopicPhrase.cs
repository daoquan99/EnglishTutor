using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Domain.Shared;

namespace EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;

/// <summary>
/// Aggregate root representing a practice phrase under a specific topic.
/// </summary>
public sealed class TopicPhrase : AggregateRoot
{
    public Guid TopicId { get; private set; }
    public string Phrase { get; private set; } = null!;
    public string PhraseNormalized { get; private set; } = null!;
    public string Translation { get; private set; } = null!;
    public string? Context { get; private set; }
    public bool IsActive { get; private set; }

    private TopicPhrase()
    {
    }

    public static TopicPhrase Create(
        Guid topicId,
        string phrase,
        string translation,
        string? context)
    {
        if (topicId == Guid.Empty)
        {
            throw new ArgumentException("TopicId cannot be empty.", nameof(topicId));
        }

        if (string.IsNullOrWhiteSpace(phrase))
        {
            throw new ArgumentException("Phrase cannot be empty.", nameof(phrase));
        }

        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new ArgumentException("Translation cannot be empty.", nameof(translation));
        }

        return new TopicPhrase
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            Phrase = phrase.Trim(),
            PhraseNormalized = TextNormalizer.Normalize(phrase),
            Translation = translation.Trim(),
            Context = context?.Trim(),
            IsActive = true
        };
    }

    public void Update(
        string phrase,
        string translation,
        string? context,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(phrase))
        {
            throw new ArgumentException("Phrase cannot be empty.", nameof(phrase));
        }

        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new ArgumentException("Translation cannot be empty.", nameof(translation));
        }

        Phrase = phrase.Trim();
        PhraseNormalized = TextNormalizer.Normalize(phrase);
        Translation = translation.Trim();
        Context = context?.Trim();
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
