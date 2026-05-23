# Workflow — Exercise completion

User completes an exercise set (multi-choice / fill-blank / translation / conjugation / ordering). Some question types are AI-graded; others are graded in-domain.

## Modules involved

Exercises (owner), Users, LearningContent (for attached lesson context), AI (free-response grading), Mistakes, Progress, StudyPlans, AdminReports.

## Contracts used

- `Users.Contracts` — target language + level.
- `LearningContent.Contracts` — lesson/skill metadata when the set is attached to a lesson.
- `AI.Contracts` — grading for translation / open-response questions.

## Main flow

```
[Client] ──POST /exercises/{setId}/start──> [Api]
   ↓ create UserExerciseAttempt (Active)
   ↓ OutboxMessage: ExerciseStartedIntegrationEvent
   ↓ return first question batch

[Client] ──POST /exercises/attempts/{id}/answer──> [Api]
   ↓ load UserExerciseAttempt
   ↓ grade the question
       deterministic (MCQ, fill-blank, ordering, conjugation) → in-domain
       free-form (translation, open) → AI.Contracts
   ↓ record answer + correctness + per-question score
   ↓ OutboxMessage: ExerciseQuestionAnsweredIntegrationEvent
   ↓ return correctness + explanation + next question (if any)

[Client] ──POST /exercises/attempts/{id}/complete──> [Api]
   ↓ compute final score, status → Completed
   ↓ OutboxMessage: ExerciseCompletedIntegrationEvent

[Worker]
   ↓ Mistakes: create Mistake from each incorrect ExerciseQuestionAnsweredIntegrationEvent
   ↓ Progress: EXP + streak from ExerciseCompletedIntegrationEvent
   ↓ StudyPlans: daily-target update
   ↓ AdminReports: LearningActivityReport, CommonMistakeStat
```

## Events published

- `ExerciseStartedIntegrationEvent`
- `ExerciseQuestionAnsweredIntegrationEvent` (one per question)
- `ExerciseCompletedIntegrationEvent`

## Read models / projections updated

- Progress: EXP, streak, daily/weekly snapshot.
- Mistakes: per-incorrect `Mistake` rows.
- StudyPlans: daily exercise-completion counter.
- AdminReports: `LearningActivityReport`, `CommonMistakeStat`.

## Failure / retry behavior

- AI grading failure on a free-form answer: return a typed error; the answer is not recorded; client retries. The attempt remains Active.
- A user abandoning mid-attempt: attempt stays Active until either the user resumes or a cleanup job (out of scope here) marks it Abandoned.
- Outbox retry standard.

## Notes

- One Mistake event per incorrect question, not per attempt — granular review.
- Question type is a discriminated value object in `Exercises.Domain/Shared/`. Adding a new type means updating the domain, the integration event payload, and `docs/api/exercises.md` + FE renderer.
- Don't fold mistakes back into the attempt aggregate beyond per-question correctness. The aggregated mistake review state belongs to Mistakes.
