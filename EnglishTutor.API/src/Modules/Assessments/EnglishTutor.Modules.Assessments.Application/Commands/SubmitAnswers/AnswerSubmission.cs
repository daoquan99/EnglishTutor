namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAnswers;

public sealed record AnswerSubmission(Guid QuestionId, string UserAnswer);
