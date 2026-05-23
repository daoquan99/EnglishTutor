# Exercises API

## GET `/api/exercises`

- Purpose: List published exercise sets for the current learner.
- Auth requirement: Authenticated user.
- Request body: None.
- Query: `page`, `pageSize`, `level`, `type`, `topic`, `skill`, `targetLanguageCode`.
- Response body: `ExerciseListResponse[]` with id, language, level, topic, skill, exercise type, title, description, total question count.
- Validation rules: Pagination defaults to `page=1`, `pageSize=20`; max page size is 100.
- Error codes: Standard auth errors.
- Application flow: Resolve user target language from Users contract when query language is not supplied, then list published sets owned by Exercises.
- Related modules: Exercises, Users.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.

## GET `/api/exercises/{id}`

- Purpose: Get an exercise set with questions and selectable options.
- Auth requirement: Authenticated user.
- Request body: None.
- Response body: `ExerciseDetailResponse`.
- Validation rules: Exercise must exist and be published.
- Error codes: `Error.NotFound`, `Error.Conflict`.
- Application flow: Load exercise set with questions/options. Correct answers and option correctness are hidden.
- Related modules: Exercises.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.

## POST `/api/exercises/{id}/attempts`

- Purpose: Start a new exercise attempt.
- Auth requirement: Authenticated user.
- Request body: None.
- Response body: `StartExerciseAttemptResponse`.
- Validation rules: Exercise set must exist, be published, and contain questions.
- Error codes: `Error.NotFound`, `Error.Conflict`.
- Application flow: Load exercise, resolve user target language, create `UserExerciseAttempt`, save domain event to Exercises outbox.
- Related modules: Exercises, Users.
- Integration events produced: `ExerciseStartedIntegrationEvent`.
- Integration events consumed: None.
- Read models/projections updated: None.

## POST `/api/exercises/attempts/{attemptId}/answers`

- Purpose: Submit and grade one answer.
- Auth requirement: Authenticated user and attempt owner.
- Request body: `{ "questionId": "guid", "userAnswer": "string" }`.
- Response body: `SubmitAnswerResponse`.
- Validation rules: Attempt must be in progress, owned by current user, question must belong to the attempt exercise set, duplicate answers are rejected.
- Error codes: `Error.NotFound`, `Error.Conflict`, `Error.Forbidden`, `Error.Validation`.
- Application flow: Static questions are graded locally; AI-graded questions call `AI.Contracts.IAssessmentGradingService`; the answer is recorded and an outbox event is saved.
- Related modules: Exercises, AI.
- Integration events produced: `ExerciseQuestionAnsweredIntegrationEvent`.
- Integration events consumed: None.
- Read models/projections updated: None.

## POST `/api/exercises/attempts/{attemptId}/complete`

- Purpose: Complete an attempt and calculate final score.
- Auth requirement: Authenticated user and attempt owner.
- Request body: None.
- Response body: `ExerciseResultResponse`.
- Validation rules: Attempt must be owned by the user; all questions must be answered before completion.
- Error codes: `Error.NotFound`, `Error.Conflict`, `Error.Forbidden`.
- Application flow: Calculates score, stores `UserExerciseResult`, includes wrong-answer details in `ExerciseCompletedIntegrationEvent`.
- Related modules: Exercises, Mistakes, Progress.
- Integration events produced: `ExerciseCompletedIntegrationEvent`.
- Integration events consumed: None.
- Read models/projections updated: Progress activity/skill/dashboard projections are updated asynchronously by Progress consumer; Mistakes cards are created asynchronously by Mistakes consumer.

## GET `/api/exercises/attempts/{attemptId}/result`

- Purpose: Get completed attempt details.
- Auth requirement: Authenticated user and attempt owner.
- Request body: None.
- Response body: `ExerciseResultResponse` with user answers, scores, correct answers, explanations, and feedback.
- Validation rules: Attempt must be completed.
- Error codes: `Error.NotFound`, `Error.Forbidden`.
- Application flow: Load attempt answers and exercise questions; return detailed result including correct answers.
- Related modules: Exercises.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.
