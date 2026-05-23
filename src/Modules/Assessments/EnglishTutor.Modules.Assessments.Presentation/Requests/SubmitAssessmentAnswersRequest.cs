namespace EnglishTutor.Modules.Assessments.Presentation.Requests;

public sealed record SubmitAssessmentAnswersRequest(IReadOnlyList<AssessmentAnswerRequest> Answers);

public sealed record AssessmentAnswerRequest(Guid QuestionId, string UserAnswer);
