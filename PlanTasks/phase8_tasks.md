# Phase 8: Assessments — Detailed Tasks

> **Goal:** Placement tests, level-up tests, AI grading with rubrics, pass/fail rules, level promotion chain
> **Dependencies:** Phase 3 (AI.Contracts), Phase 4 (Progress), Phase 2 (Users level update)
> **Estimated tasks:** 9

---

## Task 8.1: Assessments.Domain

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Assessments/EnglishTutor.Modules.Assessments.Domain/
├── Entities/
│   ├── AssessmentDefinition.cs
│   ├── AssessmentSection.cs
│   ├── AssessmentQuestion.cs
│   ├── UserAssessmentAttempt.cs
│   ├── UserAssessmentAnswer.cs
│   ├── AssessmentGradingResult.cs
│   └── AssessmentRubric.cs
├── Enums/
│   ├── AssessmentType.cs
│   ├── AssessmentSkill.cs
│   └── AssessmentAttemptStatus.cs
├── Events/
│   ├── AssessmentStartedDomainEvent.cs
│   ├── AssessmentPassedDomainEvent.cs
│   └── AssessmentFailedDomainEvent.cs
├── Rules/
│   └── AssessmentPassRule.cs
└── Errors/
    └── AssessmentErrors.cs
```

**AssessmentType enum:** `PlacementTest, LevelUpTest, SkillCheck, MonthlyReviewTest`
**AssessmentSkill enum:** `Grammar, Vocabulary, Reading, Writing, Speaking, Listening`
**AssessmentAttemptStatus enum:** `InProgress, Submitted, Grading, Passed, Failed`

**AssessmentDefinition (AggregateRoot<Guid>):**
- `AssessmentType`, `TargetLanguageCode (string)`, `ForLevel (LanguageLevel)`, `Title`, `Description`
- `PassingScore (int, default 75)`, `MinSkillScore (int, default 60)`, `TimeLimitMinutes (int?)`, `IsActive (bool)`, `CreatedAtUtc`
- Navigation: `List<AssessmentSection>`

**AssessmentSection (Entity<Guid>):**
- `AssessmentDefinitionId`, `Skill (AssessmentSkill)`, `Title`, `Weight (decimal 0.0-1.0)`, `Order (int)`
- Navigation: `List<AssessmentQuestion>`

**AssessmentQuestion (Entity<Guid>):**
- `SectionId (Guid)`, `Prompt (string)`, `QuestionType (string)`, `CorrectAnswer (string?)`, `IsAiGraded (bool)`, `MaxScore (int)`, `Order (int)`, `Explanation (string?)`

**UserAssessmentAttempt (AggregateRoot<Guid>):**
- `UserId (Guid)`, `AssessmentDefinitionId (Guid)`, `TargetLanguageCode`, `CurrentLevel (string — level when started)`, `TotalScore (int?)`, `Status (AssessmentAttemptStatus)`, `StartedAtUtc`, `SubmittedAtUtc (DateTime?)`, `GradedAtUtc (DateTime?)`
- Navigation: `List<UserAssessmentAnswer>`
- Factory: `static UserAssessmentAttempt Start(userId, definitionId, targetLanguage, currentLevel)` — raises `AssessmentStartedDomainEvent`
- Method: `void Submit()` — validates all questions answered → Status=Submitted
- Method: `void ApplyGradingResult(Dictionary<AssessmentSkill, int> sectionScores, int totalScore, int passingScore, int minSkillScore)`:
  - If `totalScore >= passingScore` AND all `sectionScores >= minSkillScore` → Status=Passed, raises `AssessmentPassedDomainEvent`
  - Else → Status=Failed, raises `AssessmentFailedDomainEvent`

**AssessmentPassRule : IBusinessRule:**
- Encapsulates pass/fail logic
- `bool IsBroken()` — returns true if totalScore < passingScore OR any sectionScore < minSkillScore
- `string Message` — describes which criteria failed

**AssessmentGradingResult (Entity<Guid>):**
- `AttemptId (Guid)`, `TotalScore (int)`, `SectionScoresJson (string — Dictionary<Skill, int>)`, `IsPassed (bool)`, `GradedAtUtc`, `GradingNotes (string?)`

**AssessmentRubric (Entity<Guid>):**
- `AssessmentDefinitionId`, `Skill`, `Criteria (string)`, `MaxScore (int)`, `ScoringGuide (string — for AI grader prompt)`

**UserAssessmentAnswer (Entity<Guid>):**
- `AttemptId`, `QuestionId`, `UserAnswer (string)`, `Score (int?)`, `Feedback (string?)`, `IsGraded (bool)`, `AnsweredAtUtc`

**AssessmentErrors:**
- `static Error DefinitionNotFound`, `AttemptNotFound`, `NotEligible`, `AttemptAlreadySubmitted`, `NotAllQuestionsAnswered`, `AttemptNotOwned`, `AlreadyGraded`

**Acceptance Criteria:**
- [ ] Pass/fail is domain logic, not AI decision
- [ ] AI = grader, Assessments = decision engine, Users = level owner
- [ ] Section weights sum to 1.0 per assessment
- [ ] Attempt status transitions enforced (InProgress→Submitted→Grading→Passed/Failed)
- [ ] Rubric provides scoring guide for AI grader

---

## Task 8.2: Assessments.Application

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Assessments/EnglishTutor.Modules.Assessments.Application/
├── Commands/
│   ├── StartLevelUpAssessment/
│   │   ├── StartLevelUpAssessmentCommand.cs
│   │   └── StartLevelUpAssessmentCommandHandler.cs
│   ├── SubmitAnswers/
│   │   ├── SubmitAssessmentAnswersCommand.cs
│   │   └── SubmitAssessmentAnswersCommandHandler.cs
│   └── SubmitAssessment/
│       ├── SubmitAssessmentCommand.cs
│       └── SubmitAssessmentCommandHandler.cs
├── Queries/
│   ├── GetAvailableAssessments/
│   │   ├── GetAvailableAssessmentsQuery.cs
│   │   └── GetAvailableAssessmentsQueryHandler.cs
│   ├── GetAttempt/
│   │   ├── GetAttemptQuery.cs
│   │   └── GetAttemptQueryHandler.cs
│   └── GetAttemptResult/
│       ├── GetAttemptResultQuery.cs
│       └── GetAttemptResultQueryHandler.cs
├── Abstractions/
│   ├── IAssessmentDefinitionRepository.cs
│   ├── IAssessmentAttemptRepository.cs
│   └── IAssessmentRubricRepository.cs
├── Services/
│   └── AssessmentGradingOrchestrator.cs
└── DTOs/
    ├── AvailableAssessmentResponse.cs
    ├── AttemptDetailResponse.cs
    ├── AttemptResultResponse.cs
    └── SectionScoreResponse.cs
```

**StartLevelUpAssessmentCommandHandler flow:**
1. Get user's current level via `Users.Contracts.IUserTargetLanguageReader`
2. Check eligibility (no recent failed attempt within cooldown period, e.g., 7 days)
3. Find `AssessmentDefinition` for user's current level + LevelUpTest type
4. Create `UserAssessmentAttempt.Start()`
5. Return attemptId + questions (without correct answers)

**SubmitAssessmentAnswersCommand:** `Guid AttemptId`, `List<AnswerSubmission> Answers`
**AnswerSubmission:** `Guid QuestionId`, `string UserAnswer`
**Handler:** Save all answers to attempt, validate attempt is InProgress

**SubmitAssessmentCommandHandler flow (the complex one):**
1. Load attempt with all answers + definition with sections + questions + rubrics
2. Call `attempt.Submit()` — validates all answered
3. Set Status = Grading
4. **Grade static questions:** compare answers, calculate scores
5. **Grade AI questions:** for each AI-graded question:
   - Load rubric for that section's skill
   - Build `GradingRequest` with prompt, user answer, rubric scoring guide
   - Call `AI.Contracts.IAssessmentGradingService.GradeAsync()`
   - Store score + feedback per answer
6. **Calculate section scores:** sum question scores per section, weighted
7. **Calculate total score:** sum weighted section scores
8. **Apply pass/fail:** call `attempt.ApplyGradingResult(sectionScores, totalScore, definition.PassingScore, definition.MinSkillScore)`
9. Create `AssessmentGradingResult`
10. If Passed: save `LevelUpApprovedIntegrationEvent(userId, language, currentLevel, nextLevel, score)` to outbox
11. If Failed: save `AssessmentFailedIntegrationEvent(userId, score, weakSkills)` to outbox

**Acceptance Criteria:**
- [ ] Eligibility check prevents spam attempts
- [ ] Static + AI grading combined per assessment
- [ ] Section scores weighted correctly
- [ ] Pass/fail decided by domain rule, not AI
- [ ] LevelUpApproved event includes previous + new level
- [ ] CancellationToken everywhere

---

## Task 8.3: Assessments.Infrastructure

**Agent:** Module Implementer

**Schema:** `"assessments"` | MigrationsHistoryTable `("__EFMigrationsHistory", "assessments")`

**Tables:** `AssessmentDefinitions`, `AssessmentSections`, `AssessmentQuestions`, `UserAssessmentAttempts`, `UserAssessmentAnswers`, `AssessmentGradingResults`, `AssessmentRubrics`, `OutboxMessages`

**Key indexes:**
- `AssessmentDefinition`: index on `(AssessmentType, TargetLanguageCode, ForLevel, IsActive)`
- `UserAssessmentAttempt`: index on `(UserId, AssessmentDefinitionId)`, index on `(UserId, Status, StartedAtUtc)` for eligibility check

**Seed data:** 1 placement test (A1-A2-B1 sections) + 1 level-up test (A1→A2) with rubrics

---

## Task 8.4: Assessments.Presentation — 6 Endpoints

**Agent:** Module Implementer

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/assessments/available` | Yes | List available assessments for user's level |
| POST | `/api/assessments/level-up` | Yes | Start level-up assessment |
| GET | `/api/assessments/attempts/{id}` | Yes | Get attempt with questions (no answers for active) |
| POST | `/api/assessments/attempts/{id}/answers` | Yes | Submit answers batch |
| POST | `/api/assessments/attempts/{id}/submit` | Yes | Submit for grading → get pass/fail |
| GET | `/api/assessments/attempts/{id}/result` | Yes | Get detailed result with scores |

---

## Task 8.5: Assessments.Contracts

**Agent:** Module Implementer

```
src/Modules/Assessments/EnglishTutor.Modules.Assessments.Contracts/
└── IntegrationEvents/
    ├── AssessmentStartedIntegrationEvent.cs
    ├── AssessmentCompletedIntegrationEvent.cs
    ├── AssessmentPassedIntegrationEvent.cs
    ├── AssessmentFailedIntegrationEvent.cs
    └── LevelUpApprovedIntegrationEvent.cs
```

**LevelUpApprovedIntegrationEvent:** `Guid UserId`, `string TargetLanguageCode`, `string PreviousLevel`, `string NewLevel`, `int AssessmentScore`, `Guid AssessmentAttemptId`, `DateTime ApprovedAtUtc`

**AssessmentFailedIntegrationEvent:** `Guid UserId`, `string TargetLanguageCode`, `int Score`, `List<string> WeakSkills`, `DateTime FailedAtUtc`

---

## Task 8.6: Level-Up Event Chain (Cross-Module)

**Agent:** Integration & Events Agent

**This is the most complex cross-module flow. Full chain:**

**Step 1:** Assessments → `LevelUpApprovedIntegrationEvent` → `assessments.OutboxMessages`

**Step 2:** Worker picks up → dispatches to `Users.Application.EventHandlers.LevelUpApprovedEventHandler`:
1. Check Inbox
2. Load `UserTargetLanguage` for userId + targetLanguageCode
3. Call `userTargetLanguage.UpdateLevel(newLevel)` — raises `UserLevelChangedDomainEvent`
4. Domain event mapped → `UserLevelChangedIntegrationEvent` saved to `users.OutboxMessages`
5. Mark in Inbox

**Step 3:** Worker picks up `UserLevelChangedIntegrationEvent` → dispatches to 3 consumers:

**3a. Progress.UserLevelChangedEventHandler:**
1. Check Inbox
2. Grant bonus EXP (500 EXP for level up)
3. Create `LearningActivityLog` (type=LevelUp)
4. Update `UserDashboardSnapshot` with new level
5. Mark in Inbox

**3b. LearningContent.UserLevelChangedEventHandler:**
1. Check Inbox
2. Refresh `UserLearningPathCards` — unlock new level content, keep completed cards
3. Mark in Inbox

**3c. Notifications.LevelUpEventHandler:**
1. Check Inbox
2. Create congratulations notification
3. Mark in Inbox

**Files to create/modify:**
```
src/Modules/Users/EnglishTutor.Modules.Users.Application/EventHandlers/LevelUpApprovedEventHandler.cs
src/Modules/Progress/EnglishTutor.Modules.Progress.Application/EventHandlers/UserLevelChangedEventHandler.cs
src/Modules/LearningContent/EnglishTutor.Modules.LearningContent.Application/EventHandlers/UserLevelChangedEventHandler.cs
src/Modules/Notifications/EnglishTutor.Modules.Notifications.Application/EventHandlers/LevelUpEventHandler.cs
```

**Acceptance Criteria:**
- [ ] Full chain works: Assessment passed → Users updates level → Progress + LearningContent + Notifications react
- [ ] Inbox idempotency at every step
- [ ] Level update is eventual consistency (not synchronous)
- [ ] If any handler fails, retry via Outbox — others already processed check Inbox and skip

---

## Task 8.7: Tests

**Agent:** Test Writer

**Key test cases:**
- Pass rule: score 80, all sections ≥ 60 → Passed
- Pass rule: score 80, one section = 55 → Failed (below min skill)
- Pass rule: score 70, all sections ≥ 60 → Failed (below passing)
- Eligibility: recent failed attempt within 7 days → not eligible
- Submit validates all questions answered
- AI grading mock returns rubric-based scores
- Level-up event handler updates UserTargetLanguage correctly
- Inbox prevents duplicate level updates

**Target:** At least 12 tests

---

## Task 8.8: Documentation

`docs/api/assessments.md`, `docs/workflows/level-up-assessment.md`

## Task 8.9: Architecture Tests

- Assessments must not reference Users.Infrastructure
- Level update only through events, never direct DB call

---

## Phase 8 Definition of Done

- [ ] Placement test + level-up test work end-to-end
- [ ] Static + AI grading combined, rubric-based
- [ ] Pass/fail decided by domain rules (≥75 total, ≥60 per section)
- [ ] Level-up chain: Assessments → Users → Progress + LearningContent + Notifications
- [ ] Eligibility check prevents spam attempts
- [ ] Inbox idempotency at every handler in the chain
- [ ] `dotnet build && dotnet test` passes
- [ ] At least 12 unit tests pass
- [ ] API + workflow docs created
