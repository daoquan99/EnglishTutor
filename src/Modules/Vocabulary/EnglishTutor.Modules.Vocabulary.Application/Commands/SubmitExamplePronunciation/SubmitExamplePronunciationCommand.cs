using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitExamplePronunciation;

public sealed record SubmitExamplePronunciationCommand(
    Guid UserId,
    Guid VocabularyExampleId,
    string TargetLanguageCode,
    string RecognizedText,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    string Feedback) : ICommand<PronunciationAttemptResponse>;
