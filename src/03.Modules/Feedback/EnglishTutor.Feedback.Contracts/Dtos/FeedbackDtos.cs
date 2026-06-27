using System;
using System.Collections.Generic;

namespace EnglishTutor.Feedback.Contracts.Dtos;

public sealed record GetSessionFeedbackResult(
    string Status, // Success, Pending, Failed, NotFound
    Guid? FeedbackId,
    string? Summary,
    string? Strengths,
    string? ImprovementAreas,
    int? Score,
    string? CefrLevel,
    string? FailureReasonCode,
    IReadOnlyList<CorrectionDto>? Corrections,
    IReadOnlyList<VocabularyDto>? Vocabulary,
    IReadOnlyList<MistakePatternDto>? MistakePatterns);

public sealed record CorrectionDto(
    Guid Id,
    string OriginalText,
    string CorrectedText,
    string Explanation,
    string Category,
    string Severity);

public sealed record VocabularyDto(
    Guid Id,
    string Term,
    string Meaning,
    string ExampleSentence,
    string Difficulty,
    double Confidence);

public sealed record MistakePatternDto(
    Guid Id,
    string Pattern,
    string Description,
    int Frequency);
