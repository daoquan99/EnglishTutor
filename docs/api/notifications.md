# Notifications API

## Overview

Notifications owns notification settings, user notification schedules (per-type timing/channels), notification messages, templates, delivery logs, and Inbox idempotency for consumed integration events.

Realtime push is provided via a SignalR hub at `/hubs/notifications`. When a notification is created by an event handler in the Worker host, it is published to a Redis pub/sub channel (`notifications:realtime`). A background relay service in the API host subscribes to that channel and forwards the payload to the connected user via SignalR.

## Data Model

### NotificationSettings (per user)
- `TimeZone` (string, required, IANA timezone e.g. "Asia/Ho_Chi_Minh")
- `QuietHoursEnabled` (bool)
- `QuietHoursStart` (TimeOnly, nullable)
- `QuietHoursEnd` (TimeOnly, nullable)

### UserNotificationSchedules (per user + notification type)
- `NotificationType` (enum)
- `IsEnabled` (bool)
- `InAppEnabled`, `EmailEnabled`, `PushEnabled` (per-type delivery channels)
- `PreferredTime` (TimeOnly, nullable)
- `ReminderBeforeMinutes`, `RemindAfterMinutes` (int, nullable)
- `Frequency` (enum: Daily/Weekly/BiWeekly/Monthly, nullable)
- `DayOfWeek` (nullable), `DayOfMonth` (int 1-28, nullable)
- Unique constraint: `(UserId, NotificationType, TargetLanguageCode)`

### Configurable notification types
- StudyReminder
- MissedStudyReminder
- MistakeReviewReminder
- VocabularyReviewReminder
- WeeklyProgressSummary (WeeklySummary in API)
- MonthlyProgressSummary (MonthlySummary in API)
- AssessmentReminder

### System notification types (no user schedule)
- DailyTargetCompleted
- LevelUpCongratulations

## Endpoints

### `GET /api/notifications`
- Purpose: List current user's notifications.
- Auth requirement: Authenticated user.
- Request query: `page`, `pageSize`, `isRead`.
- Response body: list of `NotificationResponse`.
- Validation rules: page defaults to 1, page size defaults to 20 and is capped at 100.
- Error codes: none for empty result.
- Application flow: query current user's notifications ordered by `ScheduledAtUtc` descending.
- Related modules: StudyPlans, Auth, Users.

### `POST /api/notifications/mark-all-read`
- Purpose: Mark all unread notifications as read for the current user.
- Auth requirement: Authenticated user.
- Response body: `int` — number of notifications marked as read.
- Error codes: none.
- Application flow: load all unread notifications for user, call `MarkAsRead()` on each, save, return count.

### `POST /api/notifications/{id}/mark-read`
- Purpose: Mark a single notification as read.
- Auth requirement: Authenticated user.
- Response body: updated `NotificationResponse`.
- Error codes: `Error.NotFound`.
- Application flow: load notification by current user and id, mark read, save.

### `GET /api/notifications/settings`
- Purpose: Get current user's notification settings including per-type schedules.
- Auth requirement: Authenticated user.
- Response body:
```json
{
  "timeZone": "Asia/Ho_Chi_Minh",
  "quietHours": {
    "enabled": false,
    "start": null,
    "end": null
  },
  "studyReminder": {
    "enabled": true,
    "channels": { "inApp": true, "email": false, "push": true },
    "time": "20:30",
    "beforeMinutes": 15,
    "afterMinutes": null,
    "frequency": null,
    "dayOfWeek": null,
    "dayOfMonth": null
  },
  "missedStudyReminder": {
    "enabled": true,
    "channels": { "inApp": true, "email": false, "push": false },
    "time": null,
    "beforeMinutes": null,
    "afterMinutes": 60,
    "frequency": null,
    "dayOfWeek": null,
    "dayOfMonth": null
  },
  "mistakeReviewReminder": {
    "enabled": true,
    "channels": { "inApp": true, "email": false, "push": false },
    "time": "19:30",
    "beforeMinutes": null,
    "afterMinutes": null,
    "frequency": "Daily",
    "dayOfWeek": null,
    "dayOfMonth": null
  },
  "vocabularyReviewReminder": { "..." },
  "weeklySummary": {
    "enabled": true,
    "channels": { "inApp": true, "email": true, "push": false },
    "time": "20:00",
    "beforeMinutes": null,
    "afterMinutes": null,
    "frequency": null,
    "dayOfWeek": "Sunday",
    "dayOfMonth": null
  },
  "monthlySummary": {
    "enabled": false,
    "channels": { "inApp": true, "email": false, "push": false },
    "time": "09:00",
    "beforeMinutes": null,
    "afterMinutes": null,
    "frequency": null,
    "dayOfWeek": null,
    "dayOfMonth": 1
  },
  "assessmentReminder": { "..." }
}
```
- Application flow: returns existing settings and schedules, or creates defaults (timezone "UTC", all schedules disabled, InApp-only channels).
- Lazy initialization: if no settings exist for user, creates settings + 7 schedule rows automatically.

### `PUT /api/notifications/settings`
- Purpose: Update current user's notification settings and all schedules.
- Auth requirement: Authenticated user.
- Request body: Same shape as GET response.
- Response body: Updated settings (same shape).
- Validation rules:
  - `timeZone`: required, max 100 characters, must be a valid IANA timezone identifier.
  - `quietHours`: when `enabled=true`, both `start` and `end` are required (HH:mm format).
  - Per schedule, when `enabled=true`:
    - At least one delivery channel must be enabled.
    - `studyReminder`: `time` required, `beforeMinutes` must be 0-180 if provided.
    - `missedStudyReminder`: `afterMinutes` must be 15-1440 if provided.
    - `mistakeReviewReminder`: `time` required, `frequency` must be Daily/Weekly/BiWeekly.
    - `vocabularyReviewReminder`: `time` required, `frequency` must be Daily/Weekly/BiWeekly.
    - `weeklySummary`: `time` required, `dayOfWeek` required and must be valid.
    - `monthlySummary`: `time` required, `dayOfMonth` required and must be 1-28.
    - `assessmentReminder`: `time` required.
- Error codes: `Error.Validation` for invalid input, `DomainException` for invariant violations.
- Application flow: load/create settings, update timezone and quiet hours, then for each of 7 schedule types: find-or-create schedule row, apply enable/disable, channels, and timing.

## SignalR Hub

### `/hubs/notifications`
- Auth requirement: Authenticated user (JWT Bearer). Token is sent via `access_token` query parameter.
- Connection: client connects with `@microsoft/signalr` HubConnectionBuilder. Auto-reconnect with backoff `[0, 2000, 5000, 10000, 30000]`.
- Groups: on connect, user is added to group `user:{userId}`. On disconnect, removed.

### Server-to-client events

#### `ReceiveNotification`
- Payload (JSON string):
```json
{
  "id": "guid",
  "type": "DailyTargetCompleted",
  "title": "Daily target completed",
  "body": "You studied 30 minutes today.",
  "scheduledAtUtc": "2026-05-23T12:00:00Z"
}
```
- Triggered when: a notification is created by an event handler and published via Redis pub/sub.
- Client behavior: invalidate notification queries, show toast notification.

## Realtime Architecture

```
Worker Host                    Redis                    API Host
 ┌─────────────┐           ┌─────────┐          ┌──────────────────┐
 │ EventHandler │──publish──│ pub/sub │──relay──▶│ SignalR Hub      │
 │   (creates   │           │ channel │          │ (pushes to       │
 │  notification)│          └─────────┘          │  connected user) │
 └─────────────┘                                 └──────────────────┘
```

- Channel: `notifications:realtime`
- Payload: `{ userId: Guid, payload: NotificationPushPayload }`
- The relay is a BackgroundService in the API host (`RealtimeNotificationRelay`).
- Push failure does not fail the event handler — it is fire-and-forget with warning logging.

## Events

- Produced: none currently.
- Consumed:
  - `UserRegisteredIntegrationEvent` — creates default settings + 7 schedule rows.
  - `PlannedStudySessionMissedIntegrationEvent` — checks MissedStudyReminder schedule, creates notification if enabled, pushes realtime.
  - `DailyStudyTargetCompletedIntegrationEvent` — always creates InApp notification (system type, no user schedule), pushes realtime.
  - `UserLevelChangedIntegrationEvent` — always creates InApp notification (system type, no user schedule), pushes realtime.
  - `ProgressSummaryReadyIntegrationEvent` — checks WeeklyProgressSummary/MonthlyProgressSummary schedule, creates notification if enabled, pushes realtime.
