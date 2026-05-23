# Notifications module

Queued outbound notifications (in-app realtime + email + push) and user notification preferences.

## Schema

`notifications`

## Aggregate roots

| Aggregate                    | Purpose                                                                                          |
| ---------------------------- | ------------------------------------------------------------------------------------------------ |
| `NotificationMessage`        | A queued notification instance: channel(s), recipient, template version, payload data, status.   |
| `NotificationTemplate`       | Versioned template: subject + body per channel + per UI language.                                |
| `NotificationSetting`        | Per-user channel preferences (in-app, email, push); per-category opt-in.                         |
| `UserNotificationSchedule`   | Recurring schedule: study reminders, weekly review prompts, etc.                                 |

## Contracts surface

No integration events produced (consumer-only module).

Realtime push to the SPA uses SignalR (`Hubs/NotificationHub` in `EnglishTutor.Api`). The Worker writes `NotificationMessage` rows and a relay (`RealtimeNotificationRelay`) pushes in-app payloads to connected clients.

## Key behaviors

Consumes events to generate notifications:

- `Auth.UserRegisteredIntegrationEvent` → welcome.
- `Speaking.SpeakingSessionCompletedIntegrationEvent` → session-summary card.
- `Assessments.AssessmentPassedIntegrationEvent` → level-up celebration.
- `StudyPlans.PlannedStudySessionMissedIntegrationEvent` → encouragement / re-engagement.
- `Vocabulary.VocabularyMasteredIntegrationEvent` (configurable threshold) → milestone.

Worker jobs:
- `StudyReminderSchedulingJob` — produces planned reminder messages from `UserNotificationSchedule`.
- Sends queued messages through channel adapters (email provider, push provider).

Each `NotificationMessage` carries the template snapshot at send time. Re-sends use the snapshot, not the current template.

## Notes for changes

- New notification category: add the template + opt-in switch in `NotificationSetting`; add the event consumer.
- Channel rollout: per-user opt-in defaults to off for new channels; existing users get a sensible default; never force-opt-in.
- In-app realtime payload changes coordinate with the FE `features/notifications/` types.
- Localization: templates are per UI language. Pick the language from Users' `UiLanguageCode` via Contract Reader.
