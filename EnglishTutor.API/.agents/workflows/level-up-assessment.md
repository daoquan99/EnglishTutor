# Workflow — Level-up assessment

User takes an assessment to be promoted to the next CEFR level. Passing is the only mechanism that mutates a user's level.

## Modules involved

Assessments (owner), Users, AI (rubric grading), Progress, Notifications, AdminReports.

## Contracts used

- `Users.Contracts` — current level + language settings.
- `AI.Contracts` — rubric grading for free-form sections.

## Main flow

```
[Client] ──POST /assessments/{definitionId}/start──> [Api]
   ↓ enforce cooldown / eligibility from prior UserAssessmentAttempts
   ↓ create UserAssessmentAttempt (snapshot rubric version, sections, time limit)
   ↓ OutboxMessage: AssessmentStartedIntegrationEvent
   ↓ return first section

[Client] ──POST /assessments/attempts/{id}/sections/{section}/submit──> [Api]
   ↓ record per-section answers

[Client] ──POST /assessments/attempts/{id}/submit──> [Api]
   ↓ Grading orchestrator (Assessments.Application):
       deterministic sections graded in-domain (MCQ, fill-blank, listening)
       free-form (writing, speaking) graded via AI.Contracts
   ↓ apply rubric weights → final score → pass / fail
   ↓ persist grading result; status → Completed
   ↓ OutboxMessage: AssessmentSubmittedIntegrationEvent
   ↓ if passed: OutboxMessage: AssessmentPassedIntegrationEvent
       (with TargetLevel, TargetLanguageCode)
     else: AssessmentFailedIntegrationEvent

[Worker]
   ↓ Users consumes AssessmentPassedIntegrationEvent:
       update UserTargetLanguage.Level
       raise UserLevelChangedIntegrationEvent
   ↓ Progress: EXP bonus on pass; pass/fail counters either way
   ↓ Notifications: celebration (pass) / encouragement (fail)
   ↓ AdminReports: AssessmentPassRateReport, DailyAiUsageReport
```

## Events published

- `AssessmentStartedIntegrationEvent`
- `AssessmentSubmittedIntegrationEvent`
- `AssessmentPassedIntegrationEvent` (when passed)
- `AssessmentFailedIntegrationEvent` (when failed)
- (downstream) `Users.UserLevelChangedIntegrationEvent` — consumed by Notifications, AdminReports.

## Read models / projections updated

- Users: `UserTargetLanguage.Level`.
- Progress: EXP, pass/fail counters.
- AdminReports: `AssessmentPassRateReport`.
- Notifications: queued congratulatory / encouragement message.

## Failure / retry behavior

- AI grading failure on a free-form section: orchestrator stores partial result; the attempt stays `Pending Grading`; a Worker retry job re-runs grading; client polls. Pass/fail event published only after all sections graded.
- Idempotency: `Users` consumer keys on event ID — re-delivering `AssessmentPassedIntegrationEvent` does not double-promote.
- Cooldown enforcement: if the user retries before the cooldown elapses, `/start` returns a typed error from the Application catalog (no domain exception).

## Notes

- **Only** this workflow may mutate a user's level. Other modules must not write to `UserTargetLanguage.Level`.
- Rubric snapshot is stored on the attempt; later edits to `AssessmentDefinition` do not change scores of completed attempts.
- Passing scores from non-level-up assessments (e.g., placement) follow the same flow but emit different event variants — keep them in `AssessmentIntegrationEvents.cs`.
