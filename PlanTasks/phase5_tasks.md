# Phase 5: StudyPlans + Notifications — Detailed Tasks

> **Goal:** Study scheduling, reminders, missed session detection, notification delivery
> **Dependencies:** Phase 4
> **Estimated tasks:** 14

---

## Task 5.1: StudyPlans.Domain

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/StudyPlans/EnglishTutor.Modules.StudyPlans.Domain/
├── Entities/
│   ├── UserStudyPlan.cs
│   ├── UserStudyWeekDay.cs
│   ├── PlannedStudySession.cs
│   └── StudyPlanTarget.cs
├── Enums/
│   ├── PlannedSessionStatus.cs
│   └── TargetPeriod.cs
├── Events/
│   ├── StudyPlanCreatedDomainEvent.cs
│   ├── StudyPlanUpdatedDomainEvent.cs
│   └── PlannedStudySessionMissedDomainEvent.cs
└── Errors/
    └── StudyPlanErrors.cs
```

**UserStudyPlan (AggregateRoot<Guid>):**
- `UserId (Guid)`, `TargetLanguageCode (LanguageCode)`, `PreferredStudyTimeUtc (TimeOnly)`, `ReminderBeforeMinutes (int)`, `TimeZoneId (string)`
- `DailyTargetMinutes (int)`, `WeeklyTargetMinutes (int)`, `MonthlyTargetMinutes (int)`, `MonthlyTargetStudyDays (int)`
- `IsActive (bool)`, `CreatedAtUtc`, `UpdatedAtUtc`
- Navigation: `List<UserStudyWeekDay>`, `List<StudyPlanTarget>`
- Factory: `static UserStudyPlan Create(userId, targetLanguageCode, preferredTime, timeZone)` — raises `StudyPlanCreatedDomainEvent`
- Method: `void Update(preferredTime, reminderMinutes, dailyTarget, weeklyTarget, monthlyTarget)` — raises `StudyPlanUpdatedDomainEvent`

**UserStudyWeekDay (Entity<Guid>):**
- `StudyPlanId (Guid)`, `DayOfWeek (DayOfWeek)`, `IsStudyDay (bool)`
- Default: Mon–Fri = true, Sat–Sun = false

**PlannedStudySession (Entity<Guid>):**
- `StudyPlanId (Guid)`, `UserId (Guid)`, `ScheduledDateUtc (DateTime)`, `Status (PlannedSessionStatus)`, `CompletedAtUtc (DateTime?)`, `CreatedAtUtc`
- Method: `void MarkMissed()` — validates Status == Planned → sets Missed, raises domain event
- Method: `void Skip()` — validates Status == Planned → sets Skipped
- Method: `void Complete()` — validates Status == Planned → sets Completed + CompletedAtUtc

**PlannedSessionStatus enum:** `Planned, Completed, Missed, Skipped`

**StudyPlanTarget (Entity<Guid>):**
- `StudyPlanId`, `Period (TargetPeriod)`, `TargetMinutes (int)`, `TargetStudyDays (int)`

**TargetPeriod enum:** `Daily, Weekly, Monthly`

**StudyPlanErrors:**
- `static Error PlanAlreadyExists`, `PlanNotFound`, `SessionNotFound`, `SessionNotPlanned`, `InvalidTimeZone`

**Acceptance Criteria:**
- [x] Session status transitions enforced (only Planned → other)
- [x] Default study week Mon–Fri
- [x] Domain events raised on create, update, missed
- [x] No infrastructure dependencies

---

## Task 5.2: StudyPlans.Application

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/StudyPlans/EnglishTutor.Modules.StudyPlans.Application/
├── Commands/
│   ├── CreateStudyPlan/
│   │   ├── CreateStudyPlanCommand.cs
│   │   ├── CreateStudyPlanCommandHandler.cs
│   │   └── CreateStudyPlanCommandValidator.cs
│   ├── UpdateStudyPlan/
│   │   ├── UpdateStudyPlanCommand.cs
│   │   ├── UpdateStudyPlanCommandHandler.cs
│   │   └── UpdateStudyPlanCommandValidator.cs
│   ├── UpdateSchedule/
│   │   ├── UpdateScheduleCommand.cs
│   │   └── UpdateScheduleCommandHandler.cs
│   └── SkipPlannedSession/
│       ├── SkipPlannedSessionCommand.cs
│       └── SkipPlannedSessionCommandHandler.cs
├── Queries/
│   ├── GetMyStudyPlan/
│   │   ├── GetMyStudyPlanQuery.cs
│   │   └── GetMyStudyPlanQueryHandler.cs
│   ├── GetMySchedule/
│   │   ├── GetMyScheduleQuery.cs
│   │   └── GetMyScheduleQueryHandler.cs
│   └── GetPlannedSessions/
│       ├── GetPlannedSessionsQuery.cs
│       └── GetPlannedSessionsQueryHandler.cs
├── Abstractions/
│   ├── IStudyPlanRepository.cs
│   └── IPlannedStudySessionRepository.cs
└── DTOs/
    ├── StudyPlanResponse.cs
    ├── WeekScheduleResponse.cs
    └── PlannedSessionResponse.cs
```

**CreateStudyPlanCommand:** `string TargetLanguageCode`, `TimeOnly PreferredStudyTime`, `int ReminderBeforeMinutes`, `string TimeZoneId`, `int DailyTargetMinutes`, `int WeeklyTargetMinutes`, `int MonthlyTargetMinutes`, `int MonthlyTargetStudyDays`, `List<DayOfWeek> StudyDays`

**CreateStudyPlanCommandHandler flow:**
1. Validate no active plan for user + target language
2. Validate TimeZoneId is valid IANA timezone
3. Create `UserStudyPlan.Create()`
4. Create `UserStudyWeekDay` for each day (from StudyDays or default Mon–Fri)
5. Generate `PlannedStudySession` for next 7 days based on study days
6. Save plan + save `StudyPlanCreatedIntegrationEvent` to outbox

**UpdateStudyPlanCommand:** `TimeOnly? PreferredStudyTime`, `int? ReminderBeforeMinutes`, `int? DailyTargetMinutes`, etc.

**UpdateScheduleCommand:** `List<DaySchedule> Days` where `DaySchedule { DayOfWeek, bool IsStudyDay }`
**Handler:** Update week days, regenerate planned sessions for next 7 days

**SkipPlannedSessionCommand:** `Guid SessionId`
**Handler:** Load session → validate owned by current user → call `session.Skip()`

**GetPlannedSessionsQuery:** `DateTime? FromDate`, `DateTime? ToDate`, `PlannedSessionStatus? Status`

**Validators (FluentValidation):**
- `CreateStudyPlanCommandValidator`: PreferredStudyTime required, TimeZoneId required + valid, ReminderBeforeMinutes 5–60, DailyTargetMinutes 5–240, at least 1 study day
- `UpdateStudyPlanCommandValidator`: same rules for non-null fields

**Acceptance Criteria:**
- [x] Only one active plan per user per target language
- [x] Plan creation auto-generates next 7 days of sessions
- [x] Schedule update regenerates future sessions
- [x] Skip only works on Planned status

---

## Task 5.3: StudyPlans.Infrastructure

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/StudyPlans/EnglishTutor.Modules.StudyPlans.Infrastructure/
├── Persistence/
│   ├── StudyPlansDbContext.cs
│   ├── Configurations/
│   │   ├── UserStudyPlanConfiguration.cs
│   │   ├── UserStudyWeekDayConfiguration.cs
│   │   ├── PlannedStudySessionConfiguration.cs
│   │   ├── StudyPlanTargetConfiguration.cs
│   │   └── OutboxMessageConfiguration.cs
│   ├── Repositories/
│   │   ├── StudyPlanRepository.cs
│   │   └── PlannedStudySessionRepository.cs
│   └── Migrations/
└── DependencyInjection.cs
```

**StudyPlansDbContext:** Schema `"studyplans"`, MigrationsHistoryTable `("__EFMigrationsHistory", "studyplans")`

**EF Configurations:**
- `UserStudyPlan`: table `studyplans.UserStudyPlans`, unique index on `(UserId, TargetLanguageCode)` where `IsActive = true`
- `UserStudyWeekDay`: table `studyplans.UserStudyWeekDays`, FK to UserStudyPlan, unique on `(StudyPlanId, DayOfWeek)`
- `PlannedStudySession`: table `studyplans.PlannedStudySessions`, index on `(UserId, ScheduledDateUtc, Status)`, index on `(Status, ScheduledDateUtc)` for missed detection query
- `StudyPlanTarget`: table `studyplans.StudyPlanTargets`, FK to UserStudyPlan

**Acceptance Criteria:**
- [x] Schema is `"studyplans"`
- [x] 4 tables + OutboxMessages configured
- [x] Unique constraint prevents duplicate active plans
- [x] Indexes optimized for missed session detection query

---

## Task 5.4: StudyPlans.Presentation — 6 Endpoints

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/StudyPlans/EnglishTutor.Modules.StudyPlans.Presentation/
├── StudyPlanEndpoints.cs
└── Requests/
    ├── CreateStudyPlanRequest.cs
    ├── UpdateStudyPlanRequest.cs
    ├── UpdateScheduleRequest.cs
    └── GetPlannedSessionsRequest.cs
```

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/study-plans/me` | Yes | Get my active study plan with week days |
| PUT | `/api/study-plans/me` | Yes | Update study plan settings |
| GET | `/api/study-plans/me/schedule` | Yes | Get week day schedule |
| PUT | `/api/study-plans/me/schedule` | Yes | Update study/rest days |
| GET | `/api/study-plans/me/planned-sessions` | Yes | Get planned sessions (query: fromDate, toDate, status) |
| POST | `/api/study-plans/me/planned-sessions/{id}/skip` | Yes | Skip a planned session |

**Acceptance Criteria:**
- [x] All 6 endpoints authenticated
- [x] `me` routes use `ICurrentUser.UserId`
- [x] Planned sessions support date range + status filters
- [x] No business logic in endpoints

---

## Task 5.5: StudyPlans.Contracts

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/StudyPlans/EnglishTutor.Modules.StudyPlans.Contracts/
└── IntegrationEvents/
    ├── StudyPlanCreatedIntegrationEvent.cs
    ├── StudyPlanUpdatedIntegrationEvent.cs
    ├── PlannedStudySessionCreatedIntegrationEvent.cs
    ├── PlannedStudySessionMissedIntegrationEvent.cs
    └── DailyStudyTargetCompletedIntegrationEvent.cs
```

**PlannedStudySessionMissedIntegrationEvent : IntegrationEvent:**
- `Guid UserId`, `Guid SessionId`, `string TargetLanguageCode`, `DateTime ScheduledDateUtc`, `DateTime MissedAtUtc`

**DailyStudyTargetCompletedIntegrationEvent : IntegrationEvent:**
- `Guid UserId`, `string TargetLanguageCode`, `int ActualMinutes`, `int TargetMinutes`, `DateTime CompletedDateUtc`

**Acceptance Criteria:**
- [x] All events past tense
- [x] Events carry enough data for Notifications/Progress to act without callback
- [x] No domain entities in contracts

---

## Task 5.6: Notifications.Domain

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Notifications/EnglishTutor.Modules.Notifications.Domain/
├── Entities/
│   ├── NotificationSetting.cs
│   ├── NotificationMessage.cs
│   ├── NotificationDeliveryLog.cs
│   └── NotificationTemplate.cs
├── Enums/
│   ├── NotificationType.cs
│   ├── NotificationChannel.cs
│   └── NotificationStatus.cs
└── Errors/
    └── NotificationErrors.cs
```

**NotificationType enum:** `StudyReminder, MissedStudyReminder, MistakeReviewReminder, VocabularyReviewReminder, WeeklyProgressSummary, MonthlyProgressSummary, AssessmentReminder, LevelUpCongratulations`

**NotificationChannel enum:** `InApp, Email, Push`
**NotificationStatus enum:** `Pending, Sent, Failed, Read`

**NotificationSetting (AggregateRoot<Guid>):**
- `UserId (Guid)`
- `StudyReminderEnabled (bool)`, `MissedStudyReminderEnabled (bool)`, `MistakeReviewReminderEnabled (bool)`, `VocabularyReviewReminderEnabled (bool)`, `WeeklySummaryEnabled (bool)`, `MonthlySummaryEnabled (bool)`
- `PreferredChannel (NotificationChannel)`
- `CreatedAtUtc`, `UpdatedAtUtc`
- Factory: `static NotificationSetting CreateDefault(Guid userId)` — all enabled, InApp
- Method: `void Update(...)` — updates individual flags

**NotificationMessage (AggregateRoot<Guid>):**
- `UserId (Guid)`, `Type (NotificationType)`, `Title (string)`, `Body (string)`, `Data (string? JSON for deep-link/metadata)`
- `IsRead (bool)`, `Channel (NotificationChannel)`, `Status (NotificationStatus)`
- `ScheduledAtUtc (DateTime)`, `SentAtUtc (DateTime?)`, `ReadAtUtc (DateTime?)`
- Factory: `static NotificationMessage Create(userId, type, title, body, channel, scheduledAt)`
- Method: `void MarkAsRead()` — sets IsRead=true, ReadAtUtc, Status=Read
- Method: `void MarkAsSent()` — sets SentAtUtc, Status=Sent
- Method: `void MarkAsFailed(string error)` — Status=Failed

**NotificationDeliveryLog (Entity<Guid>):**
- `NotificationMessageId`, `Channel`, `Status (Sent/Failed)`, `ErrorMessage`, `AttemptedAtUtc`

**NotificationTemplate (Entity<Guid>):**
- `Type (NotificationType)`, `LanguageCode (string)`, `TitleTemplate (string)`, `BodyTemplate (string)`, `IsActive (bool)`
- Templates use placeholders: `{userName}`, `{studyTime}`, `{streakDays}`, `{date}`

**Acceptance Criteria:**
- [x] Default settings: all enabled, InApp
- [x] Message status transitions enforced in domain
- [x] Templates support multi-language
- [x] No infrastructure dependencies

---

## Task 5.7: Notifications.Application

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Notifications/EnglishTutor.Modules.Notifications.Application/
├── Commands/
│   ├── MarkNotificationRead/
│   │   ├── MarkNotificationReadCommand.cs
│   │   └── MarkNotificationReadCommandHandler.cs
│   └── UpdateNotificationSettings/
│       ├── UpdateNotificationSettingsCommand.cs
│       ├── UpdateNotificationSettingsCommandHandler.cs
│       └── UpdateNotificationSettingsCommandValidator.cs
├── Queries/
│   ├── GetNotifications/
│   │   ├── GetNotificationsQuery.cs
│   │   └── GetNotificationsQueryHandler.cs
│   └── GetNotificationSettings/
│       ├── GetNotificationSettingsQuery.cs
│       └── GetNotificationSettingsQueryHandler.cs
├── EventHandlers/
│   ├── PlannedStudySessionMissedEventHandler.cs
│   ├── DailyStudyTargetCompletedEventHandler.cs
│   ├── LevelUpApprovedEventHandler.cs
│   └── UserRegisteredEventHandler.cs
├── Abstractions/
│   ├── INotificationRepository.cs
│   ├── INotificationSettingRepository.cs
│   └── INotificationTemplateRepository.cs
└── DTOs/
    ├── NotificationResponse.cs
    └── NotificationSettingsResponse.cs
```

**PlannedStudySessionMissedEventHandler flow:**
1. Check Inbox → skip if already processed
2. Load user's `NotificationSetting` → check `MissedStudyReminderEnabled`
3. If disabled → mark in Inbox, return
4. Load `NotificationTemplate` for `MissedStudyReminder` in user's UI language (get via `Users.Contracts.IUserLanguageSettingsReader`)
5. Render template with placeholders (date, time)
6. Create `NotificationMessage.Create(userId, MissedStudyReminder, renderedTitle, renderedBody, channel, UtcNow)`
7. Save notification
8. Mark event as processed in Inbox

**UserRegisteredEventHandler flow:**
1. Check Inbox
2. Create `NotificationSetting.CreateDefault(userId)`
3. Save + mark Inbox

**GetNotificationsQuery:** `int Page`, `int PageSize`, `bool? IsRead`
**Handler:** Query paginated, sorted by `ScheduledAtUtc` descending

**UpdateNotificationSettingsCommand:** all boolean flags + `PreferredChannel`

**Acceptance Criteria:**
- [x] Notifications only created if user has that type enabled
- [x] Templates rendered with correct UI language
- [x] Default settings auto-created on user registration
- [x] Inbox idempotency on all event handlers

---

## Task 5.8: Notifications.Infrastructure

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Notifications/EnglishTutor.Modules.Notifications.Infrastructure/
├── Persistence/
│   ├── NotificationsDbContext.cs
│   ├── Configurations/
│   │   ├── NotificationSettingConfiguration.cs
│   │   ├── NotificationMessageConfiguration.cs
│   │   ├── NotificationDeliveryLogConfiguration.cs
│   │   ├── NotificationTemplateConfiguration.cs
│   │   └── InboxMessageConfiguration.cs
│   ├── Repositories/
│   │   ├── NotificationRepository.cs
│   │   ├── NotificationSettingRepository.cs
│   │   └── NotificationTemplateRepository.cs
│   └── Migrations/
├── DependencyInjection.cs
└── Seed/
    └── NotificationTemplateSeedData.cs
```

**NotificationsDbContext:** Schema `"notifications"`, MigrationsHistoryTable `("__EFMigrationsHistory", "notifications")`

**EF Configurations:**
- `NotificationSetting`: table `notifications.NotificationSettings`, unique on `(UserId)`
- `NotificationMessage`: table `notifications.NotificationMessages`, index on `(UserId, IsRead, ScheduledAtUtc DESC)`, index on `(Status)`
- `NotificationDeliveryLog`: FK to NotificationMessage
- `NotificationTemplate`: unique on `(Type, LanguageCode)` where `IsActive = true`
- `InboxMessage`: unique on `(EventId, HandlerName)`

**Seed data:** Templates for each `NotificationType` × 2 languages (vi, en):
- StudyReminder: "Sắp đến giờ học!" / "Time to study!"
- MissedStudyReminder: "Bạn đã bỏ lỡ buổi học hôm nay" / "You missed today's session"
- etc.

**Acceptance Criteria:**
- [x] Schema `"notifications"`
- [x] 4 tables + InboxMessages
- [x] Seed templates for vi + en
- [x] Indexes optimized for user notification list query

---

## Task 5.9: Notifications.Presentation — 4 Endpoints

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Notifications/EnglishTutor.Modules.Notifications.Presentation/
├── NotificationEndpoints.cs
└── Requests/
    └── UpdateNotificationSettingsRequest.cs
```

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/notifications` | Yes | List notifications (query: page, pageSize, isRead) |
| POST | `/api/notifications/{id}/mark-read` | Yes | Mark as read |
| GET | `/api/notifications/settings` | Yes | Get my settings |
| PUT | `/api/notifications/settings` | Yes | Update settings |

**Acceptance Criteria:**
- [x] All 4 endpoints authenticated
- [x] Pagination on notification list
- [x] Only owner can read/update own notifications
- [x] No business logic in endpoints

---

## Task 5.10: Worker Jobs — Missed Session Detection + Study Reminder

**Agent:** Integration & Events Agent

**Files to create/modify:**

```
src/Bootstrapper/EnglishTutor.Worker/Jobs/
├── MissedSessionDetectionJob.cs
└── StudyReminderSchedulingJob.cs
```

**MissedSessionDetectionJob (Quartz, cron every 15 min):**
1. Query `studyplans.PlannedStudySessions` where `ScheduledDateUtc < UtcNow - 2 hours` AND `Status = Planned`
2. For each session: call `session.MarkMissed()`
3. Save changed sessions
4. Save `PlannedStudySessionMissedIntegrationEvent` per session to `studyplans.OutboxMessages`
5. Worker later processes outbox → Notifications handler creates missed reminder

**StudyReminderSchedulingJob (Quartz, cron every 5 min):**
1. Query active study plans where today is a study day
2. For each plan: calculate reminder time = `PreferredStudyTimeUtc - ReminderBeforeMinutes` (converted to user's timezone)
3. If reminder time falls within the next 5 minutes window AND no reminder already sent today
4. Check user's `NotificationSetting.StudyReminderEnabled`
5. Create `NotificationMessage` for study reminder
6. Save directly (in-process, since Worker owns this)

**Acceptance Criteria:**
- [x] Missed detection only marks sessions older than 2 hours
- [x] Reminder respects user timezone
- [x] Reminder only sent on study days
- [x] No duplicate reminders for same day
- [x] Jobs registered in Worker DI with correct Quartz cron

---

## Task 5.11: Outbox/Inbox Wiring

**Agent:** Integration & Events Agent

- Add `OutboxMessages` table config to `StudyPlansDbContext`
- Add `InboxMessages` table config to `NotificationsDbContext`
- Register StudyPlans outbox in Worker's `IOutboxProcessor` scan list
- Wire consumers:
  - `PlannedStudySessionMissedIntegrationEvent` → `Notifications.PlannedStudySessionMissedEventHandler`
  - `DailyStudyTargetCompletedIntegrationEvent` → `Notifications.DailyStudyTargetCompletedEventHandler` + `Progress.DailyStudyTargetCompletedEventHandler`
  - `UserRegisteredIntegrationEvent` → `Notifications.UserRegisteredEventHandler` (create default settings)

---

## Task 5.12: Unit Tests

**Agent:** Test Writer

**Files to create:**

```
tests/EnglishTutor.Modules.StudyPlans.UnitTests/
├── Domain/
│   ├── UserStudyPlanTests.cs
│   └── PlannedStudySessionTests.cs
├── Application/
│   ├── CreateStudyPlanCommandHandlerTests.cs
│   └── SkipPlannedSessionCommandHandlerTests.cs
└── Validators/
    └── CreateStudyPlanCommandValidatorTests.cs

tests/EnglishTutor.Modules.Notifications.UnitTests/
├── Domain/
│   ├── NotificationSettingTests.cs
│   └── NotificationMessageTests.cs
└── Application/
    ├── PlannedStudySessionMissedEventHandlerTests.cs
    └── MarkNotificationReadCommandHandlerTests.cs
```

**Key test cases:**
- Plan creation generates correct number of sessions (count study days in next 7 days)
- PlannedSession: Planned→Missed OK, Completed→Missed throws, Skipped→Missed throws
- Cannot create duplicate active plan for same user+language
- Validator rejects DailyTargetMinutes < 5 or > 240
- Validator rejects invalid timezone ID
- Default NotificationSetting has all flags enabled
- MissedEventHandler skips when MissedStudyReminderEnabled=false
- MissedEventHandler skips when Inbox already processed
- NotificationMessage.MarkAsRead() sets correct fields

**Target:** At least 15 tests across both modules

**Acceptance Criteria:**
- [x] Domain logic tested without infrastructure
- [x] Event handler tests mock repositories + Inbox
- [x] Validator tests cover edge cases
- [x] All tests pass

---

## Task 5.13: Architecture Tests Update

**Agent:** Architecture Guardian

**New rules:**
- StudyPlans must not reference Notifications.Infrastructure/Domain/Application
- Notifications must not reference StudyPlans.Infrastructure/Domain/Application
- Notifications may reference StudyPlans.Contracts only
- StudyPlans may reference Users.Contracts only

---

## Task 5.14: Documentation

**Agent:** Documentation Agent

**Files to create:**

```
docs/api/study-plans.md
docs/api/notifications.md
docs/workflows/study-reminder.md
```

**study-plans.md:** 6 endpoints with request/response JSON, validation rules, error codes
**notifications.md:** 4 endpoints with request/response JSON
**study-reminder.md:** Full workflow — plan creation → session generation → reminder scheduling → missed detection → notification creation → delivery

---

## Phase 5 Definition of Done

- [x] Study plan CRUD works (create, update, schedule, skip)
- [x] Planned sessions auto-generated for study days
- [x] Worker detects missed sessions every 15 min
- [x] Study reminders sent before preferred study time
- [x] Missed study notifications created via Outbox→Worker→Inbox
- [x] Notification settings respected (enable/disable per type)
- [x] Default notification settings auto-created on user registration
- [x] `dotnet build && dotnet test` passes
- [x] At least 15 unit tests pass
- [x] All docs created (2 API + 1 workflow)
