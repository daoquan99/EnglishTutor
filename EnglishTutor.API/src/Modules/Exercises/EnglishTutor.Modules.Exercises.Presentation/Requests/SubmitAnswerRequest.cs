namespace EnglishTutor.Modules.Exercises.Presentation.Requests;

public sealed record SubmitAnswerRequest(Guid QuestionId, string UserAnswer);
