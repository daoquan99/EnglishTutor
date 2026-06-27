using System;

namespace EnglishTutor.Learning.Application.Queries.TopicPhrases;

public sealed record PhraseReadModel(
    Guid Id,
    Guid TopicId,
    string Phrase,
    string Translation,
    string? Context,
    bool IsActive);
