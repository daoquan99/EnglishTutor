# Workflow — Study reminder

Periodic reminders to learners based on their `UserNotificationSchedule` and study activity. Detects missed sessions and triggers re-engagement nudges.

## Modules involved

Notifications (owner), Users, StudyPlans, Progress, AdminReports.

## Contracts used

- `Users.Contracts` — UI language, timezone, notification preferences.
- (no others — most data is consumed via integration events into Notifications' own schema)

## Main flow

There are two sub-flows: scheduled reminders and missed-session detection.

### A. Scheduled reminders (recurring)

```
[Worker] Quartz StudyReminderSchedulingJob (every N minutes)
   ↓ load UserNotificationSchedule rows whose NextFireAtUtc has passed
   ↓ for each row:
       resolve user's UI language + timezone via Users.Contracts
       pick NotificationTemplate version by category + UI language
       create NotificationMessage (queued)
       advance NextFireAtUtc using the schedule's cadence
   ↓ commit

[Worker] NotificationDispatcherJob
   ↓ load queued NotificationMessage rows (channel-by-channel)
   ↓ dispatch via channel adapter (in-app via SignalR / email / push)
   ↓ mark Sent (or Failed with retry)
```

### B. Missed-session detection

```
[Worker] MissedSessionDetectionJob (frequency: every few minutes)
   ↓ load PlannedStudySession rows where ScheduledForUtc < now - grace AND Status == Pending
   ↓ for each: set Status=Missed, OutboxMessage: PlannedStudySessionMissedIntegrationEvent

[Worker] OutboxProcessingJob → dispatch event
   ↓ Notifications consumes PlannedStudySessionMissedIntegrationEvent:
       respect NotificationSetting (channel + category opt-in)
       create re-engagement NotificationMessage
   ↓ AdminReports updates RetentionReport
```

## Events published

- `StudyPlans.PlannedStudySessionMissedIntegrationEvent`

## Read models / projections updated

- Notifications: `NotificationMessage` rows.
- AdminReports: `RetentionReport`, `LearningActivityReport`.

## Failure / retry behavior

- Quartz job concurrency: jobs use Quartz's misfire policies — if the job didn't run on time, it picks up the missed window on the next tick.
- Channel adapter failure (email provider down, push service unavailable): message stays in queue with retry count; dead-letter after `MaxRetries`.
- Idempotency on consumer side: receiving the same `PlannedStudySessionMissedIntegrationEvent` twice does not duplicate the reminder — keyed by `(EventId, ConsumerName)` via Inbox.
- Respect user opt-out: if `NotificationSetting` denies the category/channel, no message is created (drop, not queue).

## Notes

- Reminder timing is computed in the user's timezone (`UserProfile.Timezone`). E.g., "remind me at 8pm" means 8pm local, not 8pm UTC.
- Don't fan out reminders during quiet hours — Notifications stores per-user quiet-hour windows on `NotificationSetting`.
- Avoid retro-firing missed reminders that pile up after extended downtime. The scheduling job advances `NextFireAtUtc` past the current time when many fires were missed (single make-up at most).
