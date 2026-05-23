# Assessments module

Placement tests and level-up assessments graded with AI rubrics. The authority that promotes/demotes a user's level.

## Schema

`assessments`

## Aggregate roots

| Aggregate                  | Purpose                                                                                                  |
| -------------------------- | -------------------------------------------------------------------------------------------------------- |
| `AssessmentDefinition`     | Authored assessment: target language, target level, sections (vocab/grammar/listening/speaking), rubrics.|
| `UserAssessmentAttempt`    | A user's attempt: answers per section, AI grading results, final score, pass/fail.                       |

## Contracts surface

- `IntegrationEvents/` — collected in `AssessmentIntegrationEvents.cs`:
  - `AssessmentStartedIntegrationEvent`
  - `AssessmentSubmittedIntegrationEvent`
  - `AssessmentPassedIntegrationEvent` — consumed by Users (raises `UserLevelChangedIntegrationEvent`), Progress (EXP), Notifications (achievement).
  - `AssessmentFailedIntegrationEvent` — consumed by Progress, Notifications (encouragement).

## Key behaviors

- AI grading uses `AI.Contracts` (rubric-based scoring service). Free-response answers go through AI; objective answers are graded in-domain.
- Grading orchestrator (in `Application/Commands/SubmitAssessment/`) collects results across sections, applies rubric weights, decides pass/fail.
- Passing a level-up assessment is the **only** path to a level change. Level is owned by Users; Assessments raises the event, Users updates the value.
- Anti-abuse: cooldown between attempts; configurable per assessment definition.

## Notes for changes

- Adding a section type: extend `AssessmentDefinition` and `UserAssessmentAttempt` answer schemas; add the grading branch in the orchestrator.
- Rubric changes: introduce as a new version on the definition; existing in-progress attempts keep their original rubric snapshot.
- Don't let any other module directly write to a user's level — go through the assessment event.
