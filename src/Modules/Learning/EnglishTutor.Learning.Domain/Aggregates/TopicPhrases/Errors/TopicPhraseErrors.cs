using System;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Errors;

public static class TopicPhraseErrors
{
    public static Error NotFound(Guid id) =>
        new("Learning.TopicPhraseNotFound", $"Phrase item '{id}' was not found under this topic.");

    public static Error DuplicatePhrase(string phrase) =>
        new("Learning.TopicPhraseDuplicate", $"Phrase '{phrase}' already exists under this topic.");
}
