namespace EnglishTutor.Modules.Exercises.Application.Queries.GetExerciseById;

public sealed record ExerciseDetailResponse(
    Guid Id,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    string ExerciseType,
    string Title,
    string? Description,
    int TotalQuestions,
    IReadOnlyList<ExerciseQuestionResponse> Questions);

public sealed record ExerciseQuestionResponse(
    Guid Id,
    string QuestionType,
    string Prompt,
    string? Explanation,
    int Order,
    string Difficulty,
    bool IsAiGraded,
    IReadOnlyList<ExerciseOptionResponse> Options);

public sealed record ExerciseOptionResponse(
    Guid Id,
    string OptionText,
    int Order);
