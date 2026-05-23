# Study Reminder Workflow

## Overview

StudyPlans creates planned sessions. Worker jobs schedule reminders and detect missed sessions. Notifications creates user messages through direct worker scheduling or Outbox/Inbox event handling.

## Main Flow

1. User creates or updates a study plan.
2. StudyPlans interprets plan `PreferredStudyTime` as a local wall-clock time in `TimeZoneId`, generates planned sessions for the next seven local study days, then stores scheduled timestamps as UTC.
3. Worker runs `StudyReminderSchedulingJob` every five minutes.
4. Worker converts `utcNow` to each plan's `TimeZoneId`, evaluates the user's local study day/time, converts the reminder instant back to UTC, and creates a study reminder notification when the reminder window is reached and no reminder exists for that local study date.
5. Worker runs `MissedSessionDetectionJob` every fifteen minutes.
6. Planned sessions older than two hours and still `Planned` are marked `Missed`.
7. StudyPlans saves `PlannedStudySessionMissedIntegrationEvent` to its outbox.
8. Worker processes the outbox and dispatches the event.
9. Notifications checks Inbox idempotency and user settings, renders the template, creates a missed study notification, and marks Inbox processed.

## Modules Involved

- StudyPlans: plans, week schedule, planned sessions, missed detection state.
- Notifications: settings, templates, notification messages, Inbox.
- Worker: scheduled jobs and outbox processing.
- Users: UI language lookup for localized templates.

## Events Published/Consumed

- Published by StudyPlans: `StudyPlanCreatedIntegrationEvent`, `StudyPlanUpdatedIntegrationEvent`, `PlannedStudySessionCreatedIntegrationEvent`, `PlannedStudySessionMissedIntegrationEvent`.
- Consumed by Notifications: `PlannedStudySessionMissedIntegrationEvent`.

## Read Models/Projections Updated

No cross-module read model is updated in this phase. Notifications stores its own message records.

## Failure/Retry Behavior

StudyPlans outbox messages are at-least-once. Notifications handlers use Inbox with `(EventId, HandlerName)` idempotency. Failed outbox messages follow Worker retry and dead-letter policy.

Invalid or missing time zone ids are rejected when creating study plans. If a stored time zone later cannot be resolved by the worker host, the reminder candidate is skipped and logged; timestamps remain persisted in UTC.

`PreferredStudyTime` intentionally has no `Utc` suffix because a `TimeOnly` schedule preference is local until paired with `TimeZoneId` and a calendar date.
