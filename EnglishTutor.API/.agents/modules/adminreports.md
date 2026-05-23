# AdminReports module

Reporting and analytics. The one place where cross-module database views are allowed.

## Schema

`adminreports`

## Aggregate roots

These are mostly read-model / projection aggregates updated by integration events, not behavior-rich domain aggregates.

| Aggregate / Projection         | Purpose                                                                          |
| ------------------------------ | -------------------------------------------------------------------------------- |
| `UserOverviewCard`             | Per-user snapshot: level, language, last-active, EXP, streak, mistakes count.    |
| `LearningActivityReport`       | Daily activity aggregates across modules.                                        |
| `RetentionReport`              | Weekly / monthly retention buckets.                                              |
| `DailyAiUsageReport`           | Per-day AI request count, token totals, cost totals, by provider/model.          |
| `AssessmentPassRateReport`     | Per-assessment-definition pass / fail / cooldown distribution.                   |
| `CommonMistakeStat`            | Most-common mistakes across users, scoped by target language.                    |
| `AuditLog`                     | Admin-level operations audit (impersonation, content publish, role assignments).|

## Contracts surface

- `IntegrationEvents/`:
  - `ProgressSummaryReadyIntegrationEvent` — published when a periodic report is materialized (consumed by Notifications for admin email digests, if configured).

No Contract Readers — Admin UI queries this module via dedicated read endpoints.

## Key behaviors

- Consumes a wide range of integration events to update projection tables.
- Database views and materialized views are allowed here (and only here) for analytics queries.
- Worker jobs (`ReportGenerationJobs`) build periodic reports (daily / weekly / monthly).
- `AuditLog` captures admin actions taken via the admin panel — written from API admin handlers (or via `AdminAuditLogMiddleware`).

## Notes for changes

- Adding a new report: prefer a projection table updated by events. Use a view only if the query shape is awkward for an event-driven projection.
- Don't read other modules' core tables directly. Use events; backfill via Worker jobs that consume Contract Readers.
- Keep PII handling consistent — admin reports may aggregate but should not expose secrets / hashed credentials / tokens.
