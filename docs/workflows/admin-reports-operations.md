# Admin Reports And Operations Workflow

## Overview

AdminReports provides operational read models, audit logs, and dead-letter monitoring. Reporting is allowed to read projection tables and messaging tables for analytics and operations only; it must not become part of core business flows.

## Main Flow

1. Admin calls a report endpoint with `reports.read`.
2. Presentation checks permission claims.
3. Application delegates to `IAdminReportQueryService`.
4. Infrastructure reads adminreports projections or `messaging.DeadLetterMessages`.
5. For reprocess, admin calls dead-letter reprocess with `reports.manage`.
6. Infrastructure inserts a fresh outbox row into the producer module schema and marks the dead-letter as reprocessed.
7. API middleware writes an audit log for admin command endpoints after successful authorization.
8. Worker-dispatched integration events update AdminReports projections through Inbox-protected handlers.
9. Worker Quartz jobs generate weekly and monthly aggregate reports from Progress and Assessments data, then enqueue summary-ready events in the AdminReports outbox.
10. Notifications consumes summary-ready events through Inbox and creates user summary notifications.

## Detailed Steps

- Report endpoints support date range and simple filters.
- User overview sorting supports EXP, last activity, and registration date.
- User overview cards are updated incrementally from user registration, user level change, speaking completion, exercise completion, and vocabulary mastery events.
- AdminReports handlers mark `adminreports.InboxMessages` after successful projection updates.
- Dead-letter reprocess only allows known module schemas.
- Admin command endpoints (`POST`, `PUT`, `PATCH`, `DELETE` under `/api/admin`) are audited automatically with admin user id, action, target, old/new JSON context, IP address, and user agent.
- Role names are not used for authorization; permissions are checked directly.
- `WeeklyReportGenerationJob` runs every Monday at 03:00 UTC and aggregates the last 7 days into weekly `LearningActivityReport`, `AssessmentPassRateReport`, and `RetentionReport` records.
- `MonthlyReportGenerationJob` runs on the 1st of each month at 03:00 UTC and aggregates the previous calendar month into monthly report records.
- Report jobs enqueue `ProgressSummaryReadyIntegrationEvent` rows for users with activity in the reporting period in the same AdminReports transaction as the generated reports.
- Notifications creates in-app `WeeklyProgressSummary` or `MonthlyProgressSummary` notifications from those events and avoids duplicate notifications for the same user/type/scheduled date.

## Modules Involved

AdminReports, Auth, Users, Speaking, Exercises, Vocabulary, Progress, Assessments, Notifications, Messaging, Worker, producer modules that own outbox tables.

## Contracts Used

No cross-module domain contracts are exposed by AdminReports APIs. Authorization uses `Auth.Contracts.Permissions.PermissionCodes`. Projection consumers use integration event contracts from Auth, Users, Speaking, Exercises, and Vocabulary. Notifications consumes `AdminReports.Contracts.IntegrationEvents.ProgressSummaryReadyIntegrationEvent`.

Worker report generation uses module-owned DbContexts only inside the Worker host for reporting aggregation. It does not expose `IQueryable` or domain entities across module boundaries.

## Events Published/Consumed

AdminReports API does not publish new integration events. Reprocessing dead letters requeues an existing event payload into the source module outbox. Weekly/monthly report jobs publish `ProgressSummaryReadyIntegrationEvent` through the AdminReports outbox.

Consumed events:

- `UserRegisteredIntegrationEvent`
- `UserLevelChangedIntegrationEvent`
- `SpeakingSessionCompletedIntegrationEvent`
- `ExerciseCompletedIntegrationEvent`
- `VocabularyMasteredIntegrationEvent`

Published events:

- `ProgressSummaryReadyIntegrationEvent`

## Read Models/Projections Updated

`UserOverviewCards` is updated incrementally by the consumed events listed above. `AuditLogs` is updated by API middleware for admin commands. `LearningActivityReports`, `AssessmentPassRateReports`, and `RetentionReports` are updated by weekly/monthly Worker jobs. `DailyAiUsageReports` and `CommonMistakeStats` remain reporting projection tables.

## Failure/Retry Behavior

Report reads fail fast. Dead-letter reprocess validates the source module and leaves unknown or already reprocessed messages unchanged. Quartz retries failed report jobs according to Worker host behavior; report generation is idempotent at the report row level through upsert by report date and period. Summary event delivery is at-least-once through AdminReports Outbox and Notifications Inbox; notification insertion checks for an existing user/type/scheduled-date notification before inserting.
