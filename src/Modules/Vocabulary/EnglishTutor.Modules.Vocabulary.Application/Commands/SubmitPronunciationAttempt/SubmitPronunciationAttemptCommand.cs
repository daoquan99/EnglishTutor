using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitPronunciationAttempt;

public sealed record SubmitPronunciationAttemptCommand(
    Guid UserId,
    Guid VocabularyItemId,
    string TargetLanguageCode,
    string? AudioUrl,
    string RecognizedText,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    int CompletenessScore,
    string Feedback) : ICommand<PronunciationAttemptResponse>;
