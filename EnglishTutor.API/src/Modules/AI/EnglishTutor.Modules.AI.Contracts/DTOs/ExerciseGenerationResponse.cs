namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record ExerciseGenerationResponse(
    IReadOnlyList<GeneratedExerciseQuestion> Questions);

public sealed record GeneratedExerciseQuestion(
    string Prompt,
    IReadOnlyList<string> Options,
    string CorrectAnswer,
    string Explanation);
