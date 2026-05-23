# Assessments API

## GET `/api/assessments/available`

- Purpose: List available assessments for the current learner and target language.
- Auth requirement: Authenticated user.
- Request body: None.
- Query: `targetLanguageCode` optional.
- Response body: Assessment id, type, target language, level, title, passing score, minimum skill score, and time limit.
- Validation rules: User must have a matching target language.
- Error codes: `Error.NotFound` when target language is missing.
- Application flow: Presentation sends `GetAvailableAssessmentsQuery`; Application reads `Users.Contracts.IUserTargetLanguageReader`; Assessments repository returns active level-up assessments.
- Related modules: Assessments, Users.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.

## POST `/api/assessments/level-up`

- Purpose: Start a level-up assessment attempt.
- Auth requirement: Authenticated user.
- Request body: `{ "targetLanguageCode": "en" }`, optional.
- Response body: Attempt id, assessment definition id, target language, current level, status, sections, and questions.
- Validation rules: Learner must have a target language and must not have a recent failed attempt inside the cooldown window.
- Error codes: `Error.NotFound`, `Error.Conflict`.
- Application flow: Creates `UserAssessmentAttempt.Start`; saves attempt and `AssessmentStartedIntegrationEvent` through outbox.
- Related modules: Assessments, Users.
- Integration events produced: `AssessmentStartedIntegrationEvent`.
- Integration events consumed: None.
- Read models/projections updated: AdminReports may consume assessment events later.

## POST `/api/assessments/attempts/{attemptId}/answers`

- Purpose: Save answers for an in-progress assessment attempt.
- Auth requirement: Authenticated user and owner of attempt.
- Request body: `{ "answers": [{ "questionId": "...", "userAnswer": "..." }] }`.
- Response body: Current attempt detail.
- Validation rules: Attempt must be in progress; duplicate or unknown question answers are ignored.
- Error codes: `Error.NotFound`, `Error.Forbidden`, `Error.Conflict`.
- Application flow: Loads attempt and definition, validates ownership, appends missing answers.
- Related modules: Assessments.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.

## POST `/api/assessments/attempts/{attemptId}/submit`

- Purpose: Submit and grade an assessment attempt.
- Auth requirement: Authenticated user and owner of attempt.
- Request body: None.
- Response body: Attempt id, status, total score, pass flag, section scores, graded timestamp.
- Validation rules: All questions must be answered before submission.
- Error codes: `Error.NotFound`, `Error.Forbidden`, `Error.Conflict`.
- Application flow: Static questions are graded locally; AI-graded questions go through `AI.Contracts.IAssessmentGradingService`; domain pass rule decides pass/fail.
- Related modules: Assessments, AI, Users, Progress, LearningContent, Notifications.
- Integration events produced: `AssessmentCompletedIntegrationEvent`, `AssessmentPassedIntegrationEvent` or `AssessmentFailedIntegrationEvent`, and `LevelUpApprovedIntegrationEvent` when the learner can advance.
- Integration events consumed: None.
- Read models/projections updated: Progress, LearningContent, Notifications through the level-up event chain.

## GET `/api/assessments/attempts/{attemptId}`

- Purpose: Get attempt detail and questions.
- Auth requirement: Authenticated user and owner of attempt.
- Request body: None.
- Response body: Attempt detail.
- Validation rules: Attempt must exist and belong to current user.
- Error codes: `Error.NotFound`, `Error.Forbidden`.
- Application flow: Loads attempt with answers and assessment definition.
- Related modules: Assessments.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.

## GET `/api/assessments/attempts/{attemptId}/result`

- Purpose: Get final graded result.
- Auth requirement: Authenticated user and owner of attempt.
- Request body: None.
- Response body: Total score, pass flag, section scores, graded timestamp.
- Validation rules: Attempt must already be graded.
- Error codes: `Error.NotFound`, `Error.Forbidden`, `Error.Conflict`.
- Application flow: Reads persisted `AssessmentGradingResult`.
- Related modules: Assessments.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.
