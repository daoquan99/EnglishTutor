using System;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Errors;

public static class TopicVocabularyErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Learning.TopicVocabularyNotFound", $"Vocabulary item '{id}' was not found under this topic.");

    public static Error DuplicateWord(string word) =>
        Error.Conflict("Learning.TopicVocabularyDuplicate", $"Vocabulary word '{word}' already exists under this topic.");
}
