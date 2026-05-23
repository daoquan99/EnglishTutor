# Exercise Completion Workflow

## Overview

Exercises owns exercise sets, questions, attempts, answers, and final result records. Static questions are graded locally. Open-answer questions go through `AI.Contracts` so external provider logic stays inside the AI module.

## Main flow

1. User browses published exercise sets with `GET /api/exercises`.
2. User opens a set with `GET /api/exercises/{id}`; correct answers are hidden.
3. User starts an attempt with `POST /api/exercises/{id}/attempts`.
4. User submits answers one by one through `POST /api/exercises/attempts/{attemptId}/answers`.
5. Exercises grades static answers locally or calls AI grading through `IAssessmentGradingService`.
6. User completes the attempt with `POST /api/exercises/attempts/{attemptId}/complete`.
7. Exercises stores the final result and publishes `ExerciseCompletedIntegrationEvent`.
8. Mistakes creates mistake cards for wrong answers.
9. Progress grants EXP, updates skill progress, period progress, dashboard data, and streak.

## Detailed steps

- Exercises validates attempt ownership on answer, complete, and result endpoints.
- `UserExerciseAttempt` prevents duplicate answers and requires all questions before completion.
- AI-graded questions are the only questions that call `IAssessmentGradingService`; static questions never call AI.
- AI grading responses must be strict JSON with `score`, `feedback`, and `rubricScores`. Invalid provider output fails the grading call instead of defaulting to a passing score.
- Final score is `CorrectCount / TotalQuestions * 100`.
- Wrong-answer details contain prompt, user answer, correct answer, explanation, and question type so Mistakes does not need callbacks into Exercises.

## Modules involved

- Exercises: Owns exercise data and attempts.
- Users: Provides target language defaults through contract reader.
- AI: Provides answer grading through `AI.Contracts`.
- Mistakes: Consumes completion event and creates mistake cards.
- Progress: Consumes completion event and updates progress summaries.

## Contracts used

- `Users.Contracts.IUserLanguageSettingsReader`
- `AI.Contracts.IAssessmentGradingService`
- `Exercises.Contracts.ExerciseCompletedIntegrationEvent`

## Events published/consumed

- Published by Exercises: `ExerciseStartedIntegrationEvent`, `ExerciseQuestionAnsweredIntegrationEvent`, `ExerciseCompletedIntegrationEvent`.
- Consumed by Mistakes: `ExerciseCompletedIntegrationEvent`.
- Consumed by Progress: `ExerciseCompletedIntegrationEvent`.

## Read models/projections updated

- Progress updates activity logs, skill progress, period progress, dashboard snapshots, and streak state.
- Mistakes creates user mistake cards from wrong answers.

## Failure/retry behavior

- Exercises writes integration events to its module outbox in the same save as attempt/result changes.
- Worker dispatches outbox events with at-least-once delivery.
- Mistakes and Progress use Inbox checks for idempotency.
- Failed event processing follows the shared outbox retry and dead-letter behavior.
