# Workflow — Speaking session

A user holds a multi-turn voice conversation against an AI partner with per-turn correction and a final session summary.

## Modules involved

Speaking (owner), AI, LearningContent, Users, Progress, Mistakes, StudyPlans, Notifications, AdminReports.

## Contracts used

- `Users.Contracts` — language settings + level snapshot for the session.
- `LearningContent.Contracts` — conversation scenario lookup (when session is based on a scenario).
- `AI.Contracts` — transcription, turn correction, final session summary.

## Main flow

```
[Client] ──POST /speaking/sessions──> [Api]
   ↓ snapshot language+level from Users (Contract Reader)
   ↓ open SpeakingSession aggregate (Pending → Active)
   ↓ write OutboxMessage: SpeakingSessionStartedIntegrationEvent
   ↓ return sessionId + first prompt

[Client] ──POST /speaking/sessions/{id}/turns──> [Api]
   ↓ upload audio (storage service)
   ↓ AI: transcribe + grade + correct (AI.Contracts)
   ↓ append turn to aggregate, store transcript + corrections + scores
   ↓ if turn had a correction → OutboxMessage: SpeakingTurnCorrectedIntegrationEvent
   ↓ return AI reply + correction payload

[Client] ──POST /speaking/sessions/{id}/complete──> [Api]
   ↓ aggregate: Active → Completed, set CompletedAtUtc, compute scores
   ↓ AI: session summary (AI.Contracts)
   ↓ OutboxMessage: SpeakingSessionCompletedIntegrationEvent
   ↓ if scenario-based: ConversationPracticeCompletedIntegrationEvent
   ↓ return summary

[Worker] OutboxProcessingJob
   ↓ dispatches events → InboxMessages in consumers
     - Mistakes: create Mistake from each SpeakingTurnCorrectedIntegrationEvent
     - Progress: EXP + streak update on SpeakingSessionCompletedIntegrationEvent
     - StudyPlans: mark planned session completed
     - Notifications: queue session-summary card
     - AdminReports: update LearningActivityReport, DailyAiUsageReport
```

## Events published

- `SpeakingSessionStartedIntegrationEvent`
- `SpeakingTurnCorrectedIntegrationEvent` (zero or more per session)
- `SpeakingSessionCompletedIntegrationEvent`
- `ConversationPracticeCompletedIntegrationEvent` (when scenario-based)

## Read models / projections updated

- Progress: `UserExperience`, daily/weekly snapshots.
- Mistakes: `Mistake` rows per correction.
- StudyPlans: `PlannedStudySession.Status`.
- AdminReports: `LearningActivityReport`, `DailyAiUsageReport`.
- Notifications: `NotificationMessage` rows.

## Failure / retry behavior

- AI provider failure on transcription/grading: turn handler returns a typed error to the client; the turn is not recorded; the session stays Active.
- AI provider failure on session summary: session still transitions to Completed; summary is generated lazily via Worker retry job.
- Event dispatch failure: standard outbox retry with exponential backoff; dead-letter after `MaxRetries`.
- Inbox idempotency: re-delivery of `SpeakingTurnCorrectedIntegrationEvent` will not duplicate the Mistake row (keyed by event ID).

## Notes

- Language pair, level, and scenario version are snapshotted on the session at start. Mid-session profile changes don't retroactively alter the session.
- Audio storage URLs in events are short-lived; consumers resolve via storage service if they need replay.
