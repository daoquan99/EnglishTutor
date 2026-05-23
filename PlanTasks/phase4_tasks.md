# Phase 4: Speaking + Mistakes + Progress Core — Detailed Tasks

> **Goal:** Speaking practice runtime, mistake tracking via events, progress/EXP/dashboard foundation, full Outbox→Worker→Inbox pipeline
> **Dependencies:** Phase 3 (AI.Contracts, Vocabulary events)
> **Estimated tasks:** 19

> **Architecture correction:** Any module error catalog such as `SpeakingErrors`, `MistakeErrors`, or `ProgressErrors` must live in the module Application layer (`*.Application/Errors`) and use `EnglishTutor.BuildingBlocks.Application.Results.Error`. Domain must not contain `Errors/*Errors.cs` catalogs or reference Application `Result/Error`; Domain invariant failures use Domain exceptions/business rules.

---

## Task 4.1: Speaking.Domain — Session & Turn Entities

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Speaking/EnglishTutor.Modules.Speaking.Domain/
├── Entities/
│   ├── SpeakingSession.cs
│   ├── SpeakingTurn.cs
│   ├── SpeakingTurnResult.cs
│   ├── SpeakingSessionSummary.cs
│   └── ConversationPracticeResult.cs
├── Enums/
│   ├── SpeakingSessionStatus.cs
│   ├── SpeakingSessionType.cs
│   └── SpeakingTurnStatus.cs
├── Events/
│   ├── SpeakingSessionStartedDomainEvent.cs
│   ├── SpeakingTurnCorrectedDomainEvent.cs
│   └── SpeakingSessionCompletedDomainEvent.cs
├── ValueObjects/
│   └── LanguageSnapshot.cs
└── Errors/
    └── SpeakingErrors.cs
```

**SpeakingSession (AggregateRoot):**
- `UserId`, `SessionType (FreeTalk/ConversationPractice/TopicDiscussion)`, `Topic`
- **Language snapshot fields:** `NativeLanguageCodeAtStart`, `TargetLanguageCodeAtStart`, `UiLanguageCodeAtStart`, `ExplanationLanguageCodeAtStart`, `UserLevelAtStart`
- `Status (Active/Completed/Abandoned)`, `StartedAtUtc`, `CompletedAtUtc`
- Factory: `static SpeakingSession Start(userId, languageSnapshot, sessionType, topic)`
- Method: `SpeakingTurn AddTurn(string userText)` → raises `SpeakingTurnCorrectedDomainEvent` after correction
- Method: `void Complete()` → raises `SpeakingSessionCompletedDomainEvent`

**SpeakingTurn (Entity):**
- `SpeakingSessionId`, `TurnNumber`, `UserText`, `AudioUrl`, `Status (PendingCorrection/Corrected/Failed)`, `CreatedAtUtc`

**SpeakingTurnResult (Entity):**
- `SpeakingTurnId`, `UserId`, `TargetLanguageCode`, `OriginalText`, `CorrectedText`, `NaturalVersion`
- Scores: `GrammarScore`, `VocabularyScore`, `PronunciationScore`, `FluencyScore`, `TaskCompletionScore`, `OverallScore`
- `Feedback`, `FeedbackLanguageCode`, `AudioUrl`, `RecognizedText`, `WordLevelFeedbackJson`, `CreatedAtUtc`

**SpeakingSessionSummary (Entity):**
- `SpeakingSessionId`, `UserId`, `TargetLanguageCode`
- Average scores: Grammar, Vocabulary, Pronunciation, Fluency, Overall
- `TotalTurns`, `TotalMistakes`, `StrongPoints`, `WeakPoints`, `Recommendation`

**LanguageSnapshot (ValueObject):**
- `NativeLanguageCode`, `TargetLanguageCode`, `UiLanguageCode`, `ExplanationLanguageCode`, `UserLevel`

**Acceptance Criteria:**
- [x] Language snapshot captured at session start (immutable after creation)
- [x] Session status transitions enforced (can't add turn to completed session)
- [x] Domain events raised on start, correction, completion

---

## Task 4.2: Speaking.Application — Session Workflow Handlers

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Speaking/EnglishTutor.Modules.Speaking.Application/
├── Commands/
│   ├── StartSession/
│   │   ├── StartSpeakingSessionCommand.cs
│   │   └── StartSpeakingSessionCommandHandler.cs
│   ├── AddTurn/
│   │   ├── AddSpeakingTurnCommand.cs
│   │   └── AddSpeakingTurnCommandHandler.cs
│   └── CompleteSession/
│       ├── CompleteSpeakingSessionCommand.cs
│       └── CompleteSpeakingSessionCommandHandler.cs
├── Queries/
│   ├── GetSessions/
│   ├── GetSession/
│   └── GetSessionSummary/
├── Abstractions/
│   ├── ISpeakingSessionRepository.cs
│   ├── ISpeakingTurnRepository.cs
│   └── ISpeakingSessionSummaryRepository.cs
└── DTOs/
```

**StartSpeakingSessionCommandHandler flow:**
1. Call `Users.Contracts.IUserLanguageSettingsReader.GetByUserIdAsync(userId)`
2. Create `LanguageSnapshot` from user settings
3. Create `SpeakingSession.Start(userId, snapshot, type, topic)`
4. Save session → return `sessionId`

**AddSpeakingTurnCommandHandler flow:**
1. Load `SpeakingSession` → validate status is Active
2. Create `SpeakingTurn`
3. Build `CorrectionRequest` from session's language snapshot
4. Call `AI.Contracts.IEnglishCorrectionService.CorrectSentenceAsync()`
5. Create `SpeakingTurnResult` from correction response
6. Save turn + result
7. Save `SpeakingTurnCorrectedIntegrationEvent` to outbox

**CompleteSpeakingSessionCommandHandler flow:**
1. Load session with all turns and results
2. Calculate averages → create `SpeakingSessionSummary`
3. Call `session.Complete()`
4. Save summary + save `SpeakingSessionCompletedIntegrationEvent` to outbox

**Acceptance Criteria:**
- [x] Uses `Users.Contracts` for language settings (no direct DB access)
- [x] Uses `AI.Contracts` for correction (no direct Gemini call)
- [x] Outbox messages saved in same transaction as business data
- [x] Session summary calculates correct averages

---

## Task 4.3-4.4: Speaking.Infrastructure + Presentation

**Agent:** Module Implementer

**Speaking.Infrastructure:**
- `SpeakingDbContext` with schema `"speaking"`
- EF configs for 6 tables + OutboxMessages
- Repositories

**Speaking.Presentation — 6 endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/speaking/sessions` | Start session |
| GET | `/api/speaking/sessions` | List user sessions |
| GET | `/api/speaking/sessions/{id}` | Get session detail |
| POST | `/api/speaking/sessions/{id}/turns` | Add turn + get correction |
| POST | `/api/speaking/sessions/{id}/complete` | Complete session |
| GET | `/api/speaking/sessions/{id}/summary` | Get session summary |

---

## Task 4.5: Speaking.Contracts

**Agent:** Module Implementer

**Integration events:**
- `SpeakingSessionStartedIntegrationEvent`: UserId, SessionId, SessionType, TargetLanguageCode, UserLevel
- `SpeakingTurnCorrectedIntegrationEvent`: UserId, SessionId, TurnId, TargetLanguageCode, OriginalText, CorrectedText, GrammarScore, VocabularyScore, OverallScore, Mistakes (list), CorrectedAtUtc
- `SpeakingSessionCompletedIntegrationEvent`: UserId, SessionId, TargetLanguageCode, TotalTurns, OverallScore, DurationSeconds, CompletedAtUtc
- `ConversationPracticeCompletedIntegrationEvent`: similar + ConversationScenarioId

---

## Task 4.6: Mistakes.Domain — Mistake Entities

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Mistakes/EnglishTutor.Modules.Mistakes.Domain/
├── Entities/
│   ├── Mistake.cs
│   ├── MistakeReview.cs
│   ├── MistakeCategory.cs
│   └── UserMistakeCard.cs
├── Enums/
│   ├── MistakeType.cs
│   ├── MistakeSourceType.cs
│   └── MistakeStatus.cs
├── Events/
│   ├── MistakeCreatedDomainEvent.cs
│   ├── MistakeReviewedDomainEvent.cs
│   └── MistakeMasteredDomainEvent.cs
└── Errors/
    └── MistakeErrors.cs
```

**Mistake (AggregateRoot):**
- `UserId`, `TargetLanguageCode`, `NativeLanguageCode`, `ExplanationLanguageCode`
- `Type (Grammar/Vocabulary/Spelling/Pronunciation)`, `Category`
- `OriginalText`, `CorrectedText`, `Explanation`
- `SourceType (SpeakingTurn/ExerciseAnswer/VocabularyPronunciation/AssessmentAnswer)`
- `SourceId (Guid)`, `Status (New/Reviewed/Mastered)`, `NextReviewAtUtc`, `CreatedAtUtc`
- Factory: `static Mistake CreateFromCorrection(userId, sourceType, sourceId, mistakeDetail, languageContext)`
- Method: `void Review()`, `void MarkMastered()`

---

## Task 4.7-4.9: Mistakes.Application + Infrastructure + Presentation

**Agent:** Module Implementer

**Application — Event Handlers (key piece):**

```
├── EventHandlers/
│   ├── SpeakingTurnCorrectedEventHandler.cs
│   ├── ExerciseCompletedEventHandler.cs (placeholder for Phase 7)
│   ├── VocabularyPronunciationPracticedEventHandler.cs
│   └── AssessmentCompletedEventHandler.cs (placeholder for Phase 8)
```

**SpeakingTurnCorrectedEventHandler:**
1. Check Inbox: if already processed, return
2. Extract `Mistakes` list from event payload
3. For each mistake: create `Mistake.CreateFromCorrection()`
4. Save all mistakes
5. Save `MistakeCreatedIntegrationEvent` to outbox
6. Mark event as processed in Inbox

**Presentation — 5 endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/mistakes` | List user mistakes (paginated, filtered) |
| GET | `/api/mistakes/today` | Get today's mistakes for review |
| GET | `/api/mistakes/{id}` | Get mistake detail |
| POST | `/api/mistakes/{id}/review` | Mark mistake as reviewed |
| POST | `/api/mistakes/{id}/mark-mastered` | Mark as mastered |

**Infrastructure:**
- `MistakesDbContext` with schema `"mistakes"`
- InboxMessages table in `mistakes` schema
- OutboxMessages table in `mistakes` schema

---

## Task 4.10: Progress.Domain — EXP & Activity Entities

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Progress/EnglishTutor.Modules.Progress.Domain/
├── Entities/
│   ├── LearningActivityLog.cs
│   ├── UserExperience.cs
│   ├── ExperienceTransaction.cs
│   ├── UserSkillProgress.cs
│   ├── UserDailyProgress.cs
│   ├── UserWeeklyProgress.cs
│   ├── UserMonthlyProgress.cs
│   ├── UserDashboardSnapshot.cs
│   └── UserStreak.cs
├── Enums/
│   ├── ActivityType.cs
│   └── AppRank.cs
└── Errors/
    └── ProgressErrors.cs
```

**LearningActivityLog (Entity):**
- `UserId`, `TargetLanguageCode`, `ActivityType`, `ActivityId`, `StartedAtUtc`, `CompletedAtUtc`, `DurationSeconds`, `ExpEarned`, `Score`, `Result`

**ActivityType enum:** `LessonCompleted, VocabularyReviewed, VocabularyPronunciationPracticed, ExampleSentencePronunciationPracticed, SpeakingSessionCompleted, ExerciseCompleted, MistakeReviewed, AssessmentCompleted, DailyGoalCompleted`

**UserExperience (AggregateRoot):**
- `UserId`, `TargetLanguageCode`, `TotalExp`, `CurrentAppRank`, `UpdatedAtUtc`
- Method: `void GrantExp(int amount, string sourceType, Guid sourceId, string reason)`

**UserStreak (Entity):**
- `UserId`, `TargetLanguageCode`, `CurrentStreakDays`, `LongestStreakDays`, `LastActivityDateUtc`
- Method: `void RecordActivity(DateTime activityDateUtc)` — continues or breaks streak

**UserDashboardSnapshot (Entity):**
- `UserId`, `TargetLanguageCode`, `Date`
- `TotalExp`, `CurrentLevel`, `StreakDays`, `VocabularyMastered`, `TotalSpeakingSessions`, `TotalExercisesCompleted`, `TotalMistakes`, `WeakSkills`, `StrongSkills`, `LastUpdatedAtUtc`

---

## Task 4.11-4.12: Progress.Application + Infrastructure

**Agent:** Module Implementer

**Event Handlers (critical for cross-module data flow):**

```
├── EventHandlers/
│   ├── VocabularyReviewedEventHandler.cs
│   ├── VocabularyPronunciationPracticedEventHandler.cs
│   ├── SpeakingSessionCompletedEventHandler.cs
│   ├── ExerciseCompletedEventHandler.cs (placeholder)
│   ├── MistakeReviewedEventHandler.cs
│   ├── AssessmentCompletedEventHandler.cs (placeholder)
│   └── DailyStudyTargetCompletedEventHandler.cs (placeholder)
```

**VocabularyReviewedEventHandler:**
1. Check Inbox → skip if processed
2. Create `LearningActivityLog` (type=VocabularyReviewed)
3. Grant EXP (e.g., 10 EXP per review, 20 if correct)
4. Update `UserSkillProgress` for Vocabulary skill
5. Update `UserDailyProgress`
6. Update `UserDashboardSnapshot`
7. Update `UserStreak`
8. Mark in Inbox

**SpeakingSessionCompletedEventHandler:**
1. Check Inbox
2. Create `LearningActivityLog`
3. Grant EXP (e.g., 50 EXP per session, bonus for high scores)
4. Update `UserSkillProgress` for Speaking, Grammar, Vocabulary, Pronunciation
5. Update daily/dashboard/streak

**Queries — 6 endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/progress/dashboard/today` | Dashboard snapshot |
| GET | `/api/progress/weekly` | Weekly progress |
| GET | `/api/progress/monthly` | Monthly progress |
| GET | `/api/progress/skills` | Skill progress breakdown |
| GET | `/api/progress/experience` | EXP and rank |
| GET | `/api/progress/activities` | Recent activity log (paginated) |

**Infrastructure:**
- `ProgressDbContext` with schema `"progress"`
- InboxMessages table in `progress` schema
- 9 tables configured

---

## Task 4.13: Wire Full Outbox→Worker→Inbox Pipeline

**Agent:** Integration & Events Agent

**Description:** Make the complete event pipeline operational.

**Implementation steps:**
1. Update Worker `OutboxProcessingJob` to scan outbox tables from: `auth`, `users`, `vocabulary`, `speaking`, `mistakes`
2. For each pending message: deserialize → resolve `IIntegrationEventHandler<T>` → execute handler
3. Handler checks Inbox before processing
4. On success: mark OutboxMessage as Processed
5. On failure: increment RetryCount, set NextRetryAtUtc
6. If RetryCount > MaxRetryCount: move to `messaging.DeadLetterMessages`
7. Add `messaging` schema with `DeadLetterMessages` table (shared across modules)

**Test the full chain:**
```
Speaking.AddTurn → AI correction → SpeakingTurnCorrected outbox
→ Worker picks up → dispatches to:
  → Mistakes.SpeakingTurnCorrectedEventHandler (creates mistakes)
  → Progress.SpeakingTurnCorrectedEventHandler (updates skill progress)
→ Both check Inbox → process → mark in Inbox
```

**Acceptance Criteria:**
- [x] Worker processes outbox messages from all modules
- [x] Inbox prevents duplicate processing
- [x] Failed events retried with exponential backoff
- [x] Dead events moved to DeadLetterMessages after max retries
- [x] Full chain works end-to-end

---

## Task 4.14-4.15: Speaking + Vocabulary Event Handler Wiring

**Agent:** Integration & Events Agent

Wire concrete event consumers:
- `Mistakes` consumes `SpeakingTurnCorrectedIntegrationEvent` → creates mistakes
- `Mistakes` consumes `VocabularyPronunciationPracticedIntegrationEvent` → creates pronunciation mistakes if score < threshold
- `Progress` consumes `VocabularyReviewedIntegrationEvent` → updates EXP + activity log
- `Progress` consumes `SpeakingSessionCompletedIntegrationEvent` → updates EXP + skills + dashboard

---

## Task 4.16: Unit + Integration Tests

**Agent:** Test Writer

**Key tests:**
- SpeakingSession captures language snapshot at start
- SpeakingSession.Complete() calculates correct averages
- Can't add turn to completed session
- Mistake auto-created from speaking correction event
- Inbox prevents duplicate mistake creation
- EXP granted correctly from vocabulary review
- Streak continues/breaks correctly
- Dashboard snapshot updated after events
- Outbox→Inbox pipeline integration test

---

## Task 4.17-4.19: Documentation

**Agent:** Documentation Agent

**Files:**
- `docs/api/speaking.md` — 6 endpoints
- `docs/api/mistakes.md` — 5 endpoints
- `docs/api/progress.md` — 6 endpoints
- `docs/workflows/speaking-session.md` — full flow from start to completion
- `docs/workflows/vocabulary-flashcard-review.md` — review → mastery → events → progress
- `docs/events/integration-events.md` — all events defined so far with producer/consumer mapping
- `docs/events/outbox-inbox.md` — how the pipeline works, retry strategy, dead-letter behavior

---

## Phase 4 Definition of Done

- [x] Speaking full flow: start → turns with AI correction → complete → summary
- [x] Mistakes auto-created from speaking corrections via Outbox→Worker→Inbox
- [x] Progress updated: EXP, activity log, skill progress, streak, dashboard
- [x] Vocabulary review events → Progress updates (from Phase 3 wiring)
- [x] Outbox→Worker→Inbox pipeline works reliably
- [x] Dead-letter captures failed events
- [x] Dashboard query returns pre-computed snapshot
- [x] `dotnet build && dotnet test` passes
- [x] All API + workflow + event docs created
