# Admin Reports API

All endpoints require authentication and permission-based authorization. `reports.read` is required for read endpoints, and `reports.manage` is required for dead-letter operations. `admin.full_access` grants both.

## GET `/api/admin/reports/users`

- Purpose: List admin user overview cards.
- Auth requirement: `reports.read` or `admin.full_access`.
- Request body: None.
- Query: `page`, `pageSize`, `sortBy` (`totalExp`, `lastActivity`, `registeredAt`).
- Response body: User id, email, display name, target language, level, EXP, streak, activity counters, last activity, registration timestamp.
- Validation rules: Page size is capped in Application.
- Error codes: HTTP 403 when permission is missing.
- Application flow: Presentation checks permission; Application queries `IAdminReportQueryService`.
- Related modules: AdminReports.
- Integration events produced: None.
- Integration events consumed: Reporting projections may consume learning events.
- Read models/projections updated: `adminreports.UserOverviewCards`.

## GET `/api/admin/reports/ai-usage`

- Purpose: Read AI usage reports by date range and model.
- Auth requirement: `reports.read` or `admin.full_access`.
- Request body: None.
- Query: `from`, `to`, `modelType`.
- Response body: Date, model, task, request count, token counts, latency, failed requests, estimated cost.
- Validation rules: Optional date range filters.
- Error codes: HTTP 403 when permission is missing.
- Application flow: Reads `adminreports.DailyAiUsageReports`.
- Related modules: AdminReports, AI.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: `adminreports.DailyAiUsageReports`.

## GET `/api/admin/reports/learning-activity`

- Purpose: Read learning activity summary reports.
- Auth requirement: `reports.read` or `admin.full_access`.
- Request body: None.
- Query: `from`, `to`, `period`.
- Response body: Active users, speaking sessions, exercises, vocabulary reviews, lessons, assessments, study minutes.
- Validation rules: Period is optional and must match a known report period when provided.
- Error codes: HTTP 403 when permission is missing.
- Application flow: Reads `adminreports.LearningActivityReports`.
- Related modules: AdminReports, Progress.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: `adminreports.LearningActivityReports`.

## GET `/api/admin/reports/mistakes`

- Purpose: Read top mistake categories.
- Auth requirement: `reports.read` or `admin.full_access`.
- Request body: None.
- Query: `targetLanguageCode`, `top`.
- Response body: Mistake type, category, occurrences, affected users, examples.
- Validation rules: `top` is capped in Application.
- Error codes: HTTP 403 when permission is missing.
- Application flow: Reads `adminreports.CommonMistakeStats`.
- Related modules: AdminReports, Mistakes.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: `adminreports.CommonMistakeStats`.

## GET `/api/admin/reports/assessments`

- Purpose: Read assessment pass/fail rates.
- Auth requirement: `reports.read` or `admin.full_access`.
- Request body: None.
- Query: `from`, `to`, `targetLanguageCode`.
- Response body: Target language, assessment type, level, attempt counts, pass rate, average score.
- Validation rules: Optional date range filters.
- Error codes: HTTP 403 when permission is missing.
- Application flow: Reads `adminreports.AssessmentPassRateReports`.
- Related modules: AdminReports, Assessments.
- Integration events produced: None.
- Integration events consumed: Assessment events for projection updates.
- Read models/projections updated: `adminreports.AssessmentPassRateReports`.

## GET `/api/admin/audit-logs`

- Purpose: Read admin audit trail.
- Auth requirement: `reports.read` or `admin.full_access`.
- Request body: None.
- Query: `page`, `pageSize`, `action`, `targetEntity`.
- Response body: Admin id, action, target entity, old/new JSON values, IP, user agent, timestamp.
- Validation rules: Action filter must match known audit actions when provided.
- Error codes: HTTP 403 when permission is missing.
- Application flow: Reads `adminreports.AuditLogs`.
- Related modules: AdminReports.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: `adminreports.AuditLogs`.

## GET `/api/admin/dead-letters`

- Purpose: Inspect failed outbox messages.
- Auth requirement: `reports.manage` or `admin.full_access`.
- Request body: None.
- Query: `page`, `pageSize`, `sourceModule`, `eventType`, `status`.
- Response body: Dead-letter id, event id/type, source module, retry count, error details, status.
- Validation rules: Status filter must match known dead-letter status when provided.
- Error codes: HTTP 403 when permission is missing.
- Application flow: Reads `messaging.DeadLetterMessages` through AdminReports infrastructure.
- Related modules: AdminReports, Messaging.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.

## POST `/api/admin/dead-letters/{id}/reprocess`

- Purpose: Move a dead-letter payload back into its producer module outbox for retry.
- Auth requirement: `reports.manage` or `admin.full_access`.
- Request body: None.
- Response body: Success envelope.
- Validation rules: Dead-letter must exist, be `Dead`, and have a known source module schema.
- Error codes: `Error.NotFound`, HTTP 403.
- Application flow: Creates a fresh outbox row in the source module schema and marks the dead-letter as reprocessed.
- Related modules: AdminReports, Messaging.
- Integration events produced: Requeued original event payload.
- Integration events consumed: None.
- Read models/projections updated: None.
