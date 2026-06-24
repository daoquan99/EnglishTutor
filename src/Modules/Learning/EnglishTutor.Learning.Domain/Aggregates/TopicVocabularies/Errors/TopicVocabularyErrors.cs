using System;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Errors;

public static class TopicVocabularyErrors
{
    public static Error NotFound(Guid id) =>
        new("Learning.TopicVocabularyNotFound", $"Vocabulary item '{id}' was not found under this topic.");

    public static Error DuplicateWord(string word) =>
        new("Learning.TopicVocabularyDuplicate", $"Vocabulary word '{word}' already exists under this topic.");
}
