# Phase 9: AdminReports + Operations — Detailed Tasks

> **Goal:** Admin dashboard, analytics views, audit logs, dead-letter monitoring, report generation
> **Dependencies:** Phases 4-8 (needs events from all learning modules)
> **Estimated tasks:** 10

---

## Task 9.1: AdminReports.Domain

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AdminReports/EnglishTutor.Modules.AdminReports.Domain/
├── Entities/
│   ├── UserOverviewCard.cs
│   ├── DailyAiUsageReport.cs
│   ├── LearningActivityReport.cs
│   ├── CommonMistakeStat.cs
│   ├── AssessmentPassRateReport.cs
│   ├── RetentionReport.cs
│   └── AuditLog.cs
├── Enums/
│   ├── AuditAction.cs
│   └── ReportPeriod.cs
└── Errors/
    └── AdminReportErrors.cs
```

**UserOverviewCard (Entity<Guid> — projection):**
- `UserId (Guid)`, `Email (string)`, `DisplayName (string)`, `TargetLanguageCode (string)`, `CurrentLevel (string)`, `TotalExp (int)`, `CurrentStreakDays (int)`, `TotalSpeakingSessions (int)`, `TotalExercisesCompleted (int)`, `TotalVocabularyMastered (int)`, `TotalMistakes (int)`, `LastActivityAtUtc (DateTime?)`, `RegisteredAtUtc (DateTime)`, `LastUpdatedAtUtc`

**DailyAiUsageReport (Entity<Guid>):**
- `ReportDate (DateOnly)`, `ModelType (string)`, `TaskType (string)`, `TotalRequests (int)`, `TotalPromptTokens (long)`, `TotalCompletionTokens (long)`, `TotalTokens (long)`, `AverageLatencyMs (int)`, `FailedRequests (int)`, `EstimatedCostUsd (decimal)`

**LearningActivityReport (Entity<Guid>):**
- `ReportDate (DateOnly)`, `Period (ReportPeriod)`, `TotalActiveUsers (int)`, `TotalSpeakingSessions (int)`, `TotalExercisesCompleted (int)`, `TotalVocabularyReviews (int)`, `TotalLessonsCompleted (int)`, `TotalAssessments (int)`, `TotalStudyMinutes (int)`

**CommonMistakeStat (Entity<Guid>):**
- `TargetLanguageCode`, `MistakeType (string)`, `Category (string)`, `OccurrenceCount (int)`, `AffectedUsers (int)`, `ExampleOriginal (string)`, `ExampleCorrected (string)`, `LastUpdatedAtUtc`

**AssessmentPassRateReport (Entity<Guid>):**
- `TargetLanguageCode`, `AssessmentType (string)`, `ForLevel (string)`, `TotalAttempts (int)`, `PassedCount (int)`, `FailedCount (int)`, `PassRate (decimal)`, `AverageScore (decimal)`, `Period (ReportPeriod)`, `ReportDate (DateOnly)`

**AuditLog (Entity<Guid>):**
- `AdminUserId (Guid)`, `Action (AuditAction)`, `TargetEntity (string)`, `TargetEntityId (string)`, `OldValue (string? JSON)`, `NewValue (string? JSON)`, `IpAddress (string?)`, `UserAgent (string?)`, `CreatedAtUtc`

**AuditAction enum:** `UserLevelOverride, ContentPublished, ContentModified, PromptTemplateUpdated, ModelRoutingChanged, AssessmentResultOverride, UserDeactivated`

**ReportPeriod enum:** `Daily, Weekly, Monthly`

**Acceptance Criteria:**
- [ ] All entities are projection/read-model friendly
- [ ] AuditLog captures old/new values for change tracking
- [ ] Report entities support daily/weekly/monthly periods

---

## Task 9.2: AdminReports.Infrastructure

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AdminReports/EnglishTutor.Modules.AdminReports.Infrastructure/
├── Persistence/
│   ├── AdminReportsDbContext.cs
│   ├── Configurations/
│   │   ├── UserOverviewCardConfiguration.cs
│   │   ├── DailyAiUsageReportConfiguration.cs
│   │   ├── LearningActivityReportConfiguration.cs
│   │   ├── CommonMistakeStatConfiguration.cs
│   │   ├── AssessmentPassRateReportConfiguration.cs
│   │   ├── RetentionReportConfiguration.cs
│   │   ├── AuditLogConfiguration.cs
│   │   └── InboxMessageConfiguration.cs
│   ├── Repositories/
│   │   ├── UserOverviewCardRepository.cs
│   │   ├── AiUsageReportRepository.cs
│   │   ├── AuditLogRepository.cs
│   │   └── ReportQueryService.cs
│   ├── Views/
│   │   └── DatabaseViewMigrations.cs
│   └── Migrations/
├── DependencyInjection.cs
└── AuditLogging/
    └── AuditLogInterceptor.cs
```

**AdminReportsDbContext:** Schema `"adminreports"`, MigrationsHistoryTable `("__EFMigrationsHistory", "adminreports")`

**Database views (cross-schema, allowed for admin/reporting per architecture rules):**

```sql
-- vw_UserOverview: aggregates from users + progress schemas
CREATE VIEW adminreports.vw_UserOverview AS
SELECT u.UserId, u.Email, u.DisplayName, ut.TargetLanguageCode, ut.CurrentLevel,
       p.TotalExp, s.CurrentStreakDays, ...
FROM users.UserProfiles u
LEFT JOIN users.UserTargetLanguages ut ON u.UserId = ut.UserId AND ut.IsActive = true
LEFT JOIN progress.UserExperience p ON u.UserId = p.UserId
LEFT JOIN progress.UserStreaks s ON u.UserId = s.UserId

-- vw_DailyAiUsage: aggregates from ai schema
CREATE VIEW adminreports.vw_DailyAiUsage AS
SELECT CAST(CreatedAtUtc AS DATE) AS ReportDate, ModelUsed, TaskType,
       COUNT(*) AS TotalRequests, SUM(TotalTokens), AVG(LatencyMs), ...
FROM ai.AiRequestLogs GROUP BY ...

-- vw_CommonMistakes: aggregates from mistakes schema
CREATE VIEW adminreports.vw_CommonMistakes AS
SELECT TargetLanguageCode, Type, Category, COUNT(*) AS OccurrenceCount,
       COUNT(DISTINCT UserId) AS AffectedUsers
FROM mistakes.Mistakes GROUP BY ...
```

**EF Configurations:**
- `UserOverviewCard`: index on `(TargetLanguageCode)`, index on `(TotalExp DESC)` for leaderboard
- `AuditLog`: index on `(AdminUserId, CreatedAtUtc)`, index on `(TargetEntity, TargetEntityId)`
- `DailyAiUsageReport`: index on `(ReportDate, ModelType)`

**Acceptance Criteria:**
- [ ] Schema is `"adminreports"`
- [ ] DB views are for reporting only, not core business
- [ ] AuditLogInterceptor captures admin actions automatically
- [ ] 7 tables + InboxMessages configured

---

## Task 9.3: AdminReports.Presentation — 6 Endpoints + 2 Dead-letter

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AdminReports/EnglishTutor.Modules.AdminReports.Presentation/
├── AdminReportEndpoints.cs
├── AuditLogEndpoints.cs
├── DeadLetterEndpoints.cs
└── Requests/
    ├── GetReportRequest.cs
    └── ReprocessDeadLetterRequest.cs
```

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/admin/reports/users` | Admin | User overview cards (paginated, sortable) |
| GET | `/api/admin/reports/ai-usage` | Admin | AI usage by date range + model |
| GET | `/api/admin/reports/learning-activity` | Admin | Activity stats by period |
| GET | `/api/admin/reports/mistakes` | Admin | Top mistake categories |
| GET | `/api/admin/reports/assessments` | Admin | Pass/fail rates by level |
| GET | `/api/admin/audit-logs` | Admin | Audit trail (paginated, filtered by action/entity) |
| GET | `/api/admin/dead-letters` | Admin | Dead-letter messages (paginated) |
| POST | `/api/admin/dead-letters/{id}/reprocess` | Admin | Reprocess dead event |

**Admin auth:** Requires `Role = Admin` claim in JWT. Return 403 for non-admin users.

**Acceptance Criteria:**
- [ ] All 8 endpoints require Admin role
- [ ] Report endpoints support date range filters
- [ ] User overview sortable by TotalExp, LastActivity, RegisteredAt
- [ ] Dead-letter reprocess moves message back to source outbox

---

## Task 9.4: AdminReports Event Handlers

**Agent:** Integration & Events Agent

**Files to create:**

```
src/Modules/AdminReports/EnglishTutor.Modules.AdminReports.Application/EventHandlers/
├── UserRegisteredEventHandler.cs
├── SpeakingSessionCompletedEventHandler.cs
├── ExerciseCompletedEventHandler.cs
├── VocabularyReviewedEventHandler.cs
├── AssessmentCompletedEventHandler.cs
├── MistakeCreatedEventHandler.cs
└── UserLevelChangedEventHandler.cs
```

Each handler:
1. Check Inbox
2. Update relevant projection table (increment counters, update stats)
3. Mark in Inbox

**Example — SpeakingSessionCompletedEventHandler:**
1. Check Inbox
2. Load `UserOverviewCard` for userId → increment `TotalSpeakingSessions`, update `LastActivityAtUtc`
3. Update `LearningActivityReport` for today → increment `TotalSpeakingSessions`
4. Mark in Inbox

**Acceptance Criteria:**
- [ ] All learning events update relevant report projections
- [ ] Inbox idempotency on all handlers
- [ ] UserOverviewCard updated incrementally, not rebuilt each time

---

## Task 9.5: Audit Logging

**Agent:** Module Implementer

**AuditLogInterceptor:** Middleware or `SaveChangesInterceptor` that:
1. Detects admin-initiated changes (check `ICurrentUser.IsAdmin`)
2. Captures entity type, entity ID, old values, new values
3. Creates `AuditLog` entry
4. Applies to: PromptTemplate changes, ModelRoutingRule changes, Assessment result overrides, User level overrides

**Acceptance Criteria:**
- [ ] Only admin actions are audited
- [ ] Old/new values captured as JSON
- [ ] IP address captured from HttpContext

---

## Task 9.6: Dead-Letter Monitoring & Reprocessing

**Agent:** Integration & Events Agent

**DeadLetterEndpoints:**
- `GET /api/admin/dead-letters`: query `messaging.DeadLetterMessages`, paginated, filterable by `EventType`, `SourceModule`, `Status`
- `POST /api/admin/dead-letters/{id}/reprocess`: change Status from Dead→Reprocessing, create new `OutboxMessage` in source module's outbox with the original payload, reset retry count

**Acceptance Criteria:**
- [ ] Admin can see all failed events with error details
- [ ] Reprocess creates fresh outbox entry for retry
- [ ] Reprocessed dead letter marked as Reprocessed

---

## Task 9.7: Worker — Report Generation Jobs

**Agent:** Integration & Events Agent

**Files to create:**

```
src/Bootstrapper/EnglishTutor.Worker/Jobs/
├── WeeklyReportGenerationJob.cs
└── MonthlyReportGenerationJob.cs
```

**WeeklyReportGenerationJob (Quartz, cron every Monday 03:00 UTC):**
1. Aggregate last 7 days of `LearningActivityLogs` → create `LearningActivityReport` (Period=Weekly)
2. Aggregate last 7 days of `AssessmentAttempts` → create `AssessmentPassRateReport` (Period=Weekly)
3. Generate `WeeklyProgressSummary` notifications for all active users

**MonthlyReportGenerationJob (Quartz, cron 1st of month 03:00 UTC):**
1. Aggregate last month data → monthly report records
2. Calculate retention: users active this month vs last month
3. Generate `MonthlyProgressSummary` notifications

**Acceptance Criteria:**
- [ ] Weekly job runs every Monday
- [ ] Monthly job runs 1st of month
- [ ] Reports aggregated correctly
- [ ] Summary notifications created for active users

---

## Task 9.8: Tests

**Agent:** Test Writer

**Key test cases:**
- UserOverviewCard increments correctly on events
- AuditLog captures old/new values
- Admin auth required (non-admin → 403)
- Dead-letter reprocess creates new outbox entry
- Report aggregation calculates correct totals

**Target:** At least 10 tests

---

## Task 9.9: Architecture Tests

- AdminReports may use DB views across schemas (explicitly allowed for reporting)
- AdminReports must not be referenced by other modules
- Other modules must not query adminreports schema

---

## Task 9.10: Documentation

`docs/api/admin-reports.md`, `docs/workflows/progress-dashboard.md`

---

## Phase 9 Definition of Done

- [ ] Admin can view user overview, AI usage, activity stats, mistake trends, assessment pass rates
- [ ] Audit logs capture admin actions with old/new values
- [ ] Dead-letter messages visible + reprocessable by admin
- [ ] Weekly/monthly report generation jobs run on schedule
- [ ] All admin endpoints require Admin role
- [ ] `dotnet build && dotnet test` passes
- [ ] At least 10 unit tests pass
- [ ] API + workflow docs created
