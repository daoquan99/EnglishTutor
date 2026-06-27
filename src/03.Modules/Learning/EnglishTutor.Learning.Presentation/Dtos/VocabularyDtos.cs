using System;

namespace EnglishTutor.Learning.Presentation.Dtos;

public sealed record CreateVocabularyRequest(
    string Word,
    string Definition,
    string? PartOfSpeech,
    string? Phonetic,
    string? ExampleSentence,
    string? ExampleTranslation);

public sealed record UpdateVocabularyRequest(
    string Word,
    string Definition,
    string? PartOfSpeech,
    string? Phonetic,
    string? ExampleSentence,
    string? ExampleTranslation,
    bool IsActive);

public sealed record VocabularyResponse(
    Guid Id,
    Guid TopicId,
    string Word,
    string Definition,
    string? PartOfSpeech,
    string? Phonetic,
    string? ExampleSentence,
    string? ExampleTranslation,
    bool IsActive);
