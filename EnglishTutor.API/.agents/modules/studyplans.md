# StudyPlans module

Personalized study plans and scheduled study sessions. Defines what the user *should* do; the actual results are owned by domain modules (Vocabulary, Exercises, Speaking).

## Schema

`studyplans`

## Aggregate roots

| Aggregate              | Purpose                                                                                       |
| ---------------------- | --------------------------------------------------------------------------------------------- |
| `UserStudyPlan`        | A user's active plan for a target language: goals, weekly cadence, daily targets.             |
| `PlannedStudySession`  | A scheduled session (date + intent). Tracks completion / missed status.                       |

## Contracts surface

- `IntegrationEvents/`:
  - `StudyPlanCreatedIntegrationEvent`
  - `StudyPlanUpdatedIntegrationEvent`
  - `PlannedStudySessionCreatedIntegrationEvent`
  - `PlannedStudySessionMissedIntegrationEvent` — produced by Worker job when a session passes its window without completion. Consumed by Notifications.
  - `DailyStudyTargetCompletedIntegrationEvent` — consumed by Progress (EXP/streak).

## Key behaviors

- Listens to completion events from Vocabulary/Exercises/Speaking to mark planned sessions as completed and compute daily-target progress (scoped by `UserId + TargetLanguageCode`).
- Worker job (`MissedSessionDetectionJob`) finds sessions past their window and raises `PlannedStudySessionMissedIntegrationEvent`.

## Notes for changes

- Daily-target date boundary uses the user's timezone (read from Users via Contract Reader). Persist the resulting business date; underlying timestamps stay UTC.
- A user has one active plan per target language. Multiple plans across target languages are allowed.
