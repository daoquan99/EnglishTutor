namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitFillBlank;

public sealed record FillBlankResultResponse(
    Guid AttemptId,
    bool IsCorrect,
    string CorrectAnswer,
    string FullSentence,
    int ExampleSentenceScore);
