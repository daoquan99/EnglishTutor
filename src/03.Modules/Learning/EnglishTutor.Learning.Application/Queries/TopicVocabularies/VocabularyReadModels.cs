using System;

namespace EnglishTutor.Learning.Application.Queries.TopicVocabularies;

public sealed record VocabularyReadModel(
    Guid Id,
    Guid TopicId,
    string Word,
    string Definition,
    string? PartOfSpeech,
    string? Phonetic,
    string? ExampleSentence,
    string? ExampleTranslation,
    bool IsActive);
