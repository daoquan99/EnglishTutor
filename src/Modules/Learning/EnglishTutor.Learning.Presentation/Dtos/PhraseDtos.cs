using System;

namespace EnglishTutor.Learning.Presentation.Dtos;

public sealed record CreatePhraseRequest(
    string Phrase,
    string Translation,
    string? Context);

public sealed record UpdatePhraseRequest(
    string Phrase,
    string Translation,
    string? Context,
    bool IsActive);

public sealed record PhraseResponse(
    Guid Id,
    Guid TopicId,
    string Phrase,
    string Translation,
    string? Context,
    bool IsActive);
