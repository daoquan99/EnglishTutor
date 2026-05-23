# Phase 7: Exercises — Detailed Tasks

> **Goal:** Exercise sets with static + AI-graded questions, attempts, scoring, event wiring to Mistakes/Progress
> **Dependencies:** Phase 3 (AI.Contracts), Phase 4 (Mistakes, Progress event handlers)
> **Estimated tasks:** 9

---

## Task 7.1: Exercises.Domain

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Exercises/EnglishTutor.Modules.Exercises.Domain/
├── Entities/
│   ├── ExerciseSet.cs
│   ├── ExerciseQuestion.cs
│   ├── ExerciseOption.cs
│   ├── UserExerciseAttempt.cs
│   ├── UserExerciseAnswer.cs
│   └── UserExerciseResult.cs
├── Enums/
│   ├── ExerciseType.cs
│   ├── QuestionDifficulty.cs
│   └── ExerciseAttemptStatus.cs
├── Events/
│   ├── ExerciseStartedDomainEvent.cs
│   ├── ExerciseQuestionAnsweredDomainEvent.cs
│   └── ExerciseCompletedDomainEvent.cs
└── Errors/
    └── ExerciseErrors.cs
```

**ExerciseType enum:** `MultipleChoice, FillInTheBlank, VerbConjugation, SentenceCorrection, SentenceOrdering, Translation, Matching, ListeningChoice, ConversationCompletion, ShortWriting`

**ExerciseAttemptStatus enum:** `InProgress, Completed`
**QuestionDifficulty enum:** `Easy, Medium, Hard`

**ExerciseSet (AggregateRoot<Guid>):**
- `TargetLanguageCode (string)`, `Level (LanguageLevel)`, `Topic (string)`, `Skill (LearningSkill)`, `ExerciseType (ExerciseType)`, `Title (string)`, `Description (string?)`, `IsPublished (bool)`, `TotalQuestions (int)`, `CreatedAtUtc`
- Navigation: `List<ExerciseQuestion>`

**ExerciseQuestion (Entity<Guid>):**
- `ExerciseSetId (Guid)`, `QuestionType (ExerciseType)`, `Prompt (string)`, `CorrectAnswer (string?)`, `Explanation (string?)`, `Order (int)`, `Difficulty (QuestionDifficulty)`, `IsAiGraded (bool)`
- Navigation: `List<ExerciseOption>` (for MultipleChoice/Matching)
- `IsAiGraded` = true for Translation, ShortWriting, ConversationCompletion

**ExerciseOption (Entity<Guid>):**
- `QuestionId (Guid)`, `OptionText (string)`, `IsCorrect (bool)`, `Order (int)`

**UserExerciseAttempt (AggregateRoot<Guid>):**
- `UserId (Guid)`, `ExerciseSetId (Guid)`, `TargetLanguageCode (string)`, `StartedAtUtc (DateTime)`, `CompletedAtUtc (DateTime?)`, `TotalQuestions (int)`, `CorrectCount (int)`, `Score (int)`, `Status (ExerciseAttemptStatus)`
- Navigation: `List<UserExerciseAnswer>`
- Factory: `static UserExerciseAttempt Start(userId, exerciseSetId, targetLanguageCode, totalQuestions)` — raises `ExerciseStartedDomainEvent`
- Method: `UserExerciseAnswer RecordAnswer(Guid questionId, string userAnswer, bool isCorrect, int score, string? feedback)` — creates answer, updates CorrectCount
- Method: `void Complete()` — validates all questions answered → calculates Score → sets Status=Completed → raises `ExerciseCompletedDomainEvent`
- Business rule: Score = `(CorrectCount / TotalQuestions) * 100`

**UserExerciseAnswer (Entity<Guid>):**
- `AttemptId (Guid)`, `QuestionId (Guid)`, `UserAnswer (string)`, `IsCorrect (bool)`, `Score (int 0-100)`, `Feedback (string?)`, `AnsweredAtUtc (DateTime)`

**UserExerciseResult (Entity<Guid>):**
- `AttemptId (Guid)`, `UserId (Guid)`, `ExerciseSetId (Guid)`, `TargetLanguageCode`, `ExerciseType`, `TotalScore (int)`, `CorrectCount`, `TotalQuestions`, `TimeTakenSeconds (int)`, `CompletedAtUtc`

**ExerciseErrors:**
- `static Error ExerciseSetNotFound`, `AttemptNotFound`, `AttemptAlreadyCompleted`, `QuestionNotFound`, `QuestionAlreadyAnswered`, `NotAllQuestionsAnswered`

**Acceptance Criteria:**
- [x] Attempt tracks correct count incrementally
- [x] Score calculated as percentage on completion
- [x] Cannot complete with unanswered questions
- [x] Cannot answer same question twice
- [x] AI-graded flag on question level
- [x] Domain events on start, answer, complete

---

## Task 7.2: Exercises.Application

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Exercises/EnglishTutor.Modules.Exercises.Application/
├── Commands/
│   ├── StartAttempt/
│   │   ├── StartExerciseAttemptCommand.cs
│   │   └── StartExerciseAttemptCommandHandler.cs
│   ├── SubmitAnswer/
│   │   ├── SubmitAnswerCommand.cs
│   │   ├── SubmitAnswerCommandHandler.cs
│   │   └── SubmitAnswerCommandValidator.cs
│   └── CompleteExercise/
│       ├── CompleteExerciseCommand.cs
│       └── CompleteExerciseCommandHandler.cs
├── Queries/
│   ├── GetExercises/
│   │   ├── GetExercisesQuery.cs
│   │   └── GetExercisesQueryHandler.cs
│   ├── GetExerciseById/
│   │   ├── GetExerciseByIdQuery.cs
│   │   └── GetExerciseByIdQueryHandler.cs
│   └── GetAttemptResult/
│       ├── GetAttemptResultQuery.cs
│       └── GetAttemptResultQueryHandler.cs
├── Abstractions/
│   ├── IExerciseSetRepository.cs
│   ├── IExerciseAttemptRepository.cs
│   └── IExerciseAnswerRepository.cs
├── Services/
│   └── ExerciseGradingService.cs
└── DTOs/
    ├── ExerciseListResponse.cs
    ├── ExerciseDetailResponse.cs
    ├── SubmitAnswerResponse.cs
    └── ExerciseResultResponse.cs
```

**StartExerciseAttemptCommand:** `Guid ExerciseSetId`
**Handler flow:**
1. Load ExerciseSet → validate exists, published
2. Get user's target language from `ICurrentUser`
3. Create `UserExerciseAttempt.Start(userId, setId, language, totalQuestions)`
4. Save → return attemptId

**SubmitAnswerCommand:** `Guid AttemptId`, `Guid QuestionId`, `string UserAnswer`
**Handler flow:**
1. Load attempt → validate InProgress, owned by current user
2. Load question → validate belongs to attempt's exercise set
3. Check question not already answered
4. **If static question** (`IsAiGraded == false`):
   - Compare `UserAnswer` with `CorrectAnswer` (case-insensitive, trimmed)
   - For MultipleChoice: match selected option IsCorrect
   - For FillInTheBlank: exact/fuzzy match with CorrectAnswer
   - For SentenceOrdering: compare ordered words
   - Set `IsCorrect`, `Score = IsCorrect ? 100 : 0`
5. **If AI-graded question** (`IsAiGraded == true`):
   - Build `GradingRequest` with question prompt, user answer, correct answer reference
   - Call `AI.Contracts.IAssessmentGradingService.GradeAnswerAsync()`
   - Get Score (0-100) + Feedback from AI
   - Set `IsCorrect = Score >= 70`
6. Call `attempt.RecordAnswer(questionId, userAnswer, isCorrect, score, feedback)`
7. Save answer → return `SubmitAnswerResponse` with isCorrect, score, feedback, explanation

**CompleteExerciseCommand:** `Guid AttemptId`
**Handler flow:**
1. Load attempt with all answers → validate InProgress
2. Validate all questions answered (`answers.Count == TotalQuestions`)
3. Call `attempt.Complete()` → calculates overall score
4. Create `UserExerciseResult`
5. Collect wrong answers: `{QuestionId, UserAnswer, CorrectAnswer, Explanation}`
6. Save `ExerciseCompletedIntegrationEvent` to outbox (includes wrong answers list)
7. Return result summary

**ExerciseGradingService:**
- Encapsulates static grading logic per ExerciseType
- `GradeStaticAnswer(ExerciseType type, string userAnswer, string correctAnswer) → (bool isCorrect, int score)`

**Acceptance Criteria:**
- [x] Static grading works for all 5 MVP types
- [x] AI grading called only for AI-graded questions
- [x] Cannot submit answer for completed attempt
- [x] Cannot complete with missing answers
- [x] Wrong answers included in completion event for Mistakes module
- [x] CancellationToken on all async methods

---

## Task 7.3: Exercises.Infrastructure

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Exercises/EnglishTutor.Modules.Exercises.Infrastructure/
├── Persistence/
│   ├── ExercisesDbContext.cs
│   ├── Configurations/
│   │   ├── ExerciseSetConfiguration.cs
│   │   ├── ExerciseQuestionConfiguration.cs
│   │   ├── ExerciseOptionConfiguration.cs
│   │   ├── UserExerciseAttemptConfiguration.cs
│   │   ├── UserExerciseAnswerConfiguration.cs
│   │   ├── UserExerciseResultConfiguration.cs
│   │   └── OutboxMessageConfiguration.cs
│   ├── Repositories/
│   │   ├── ExerciseSetRepository.cs
│   │   ├── ExerciseAttemptRepository.cs
│   │   └── ExerciseAnswerRepository.cs
│   └── Migrations/
├── DependencyInjection.cs
└── Seed/
    └── ExerciseSeedData.cs
```

**ExercisesDbContext:** Schema `"exercises"`, MigrationsHistoryTable `("__EFMigrationsHistory", "exercises")`

**EF Configurations:**
- `ExerciseSet`: table `exercises.ExerciseSets`, index on `(TargetLanguageCode, Level, ExerciseType, IsPublished)`
- `ExerciseQuestion`: FK to ExerciseSet, ordered by `Order`
- `ExerciseOption`: FK to ExerciseQuestion, ordered by `Order`
- `UserExerciseAttempt`: index on `(UserId, ExerciseSetId)`, index on `(UserId, Status)`
- `UserExerciseAnswer`: FK to Attempt, unique on `(AttemptId, QuestionId)` — prevents double answer
- `UserExerciseResult`: FK to Attempt, index on `(UserId, TargetLanguageCode)`

**Seed data:**
- 2 MultipleChoice sets (5 questions each, 4 options per question)
- 2 FillInTheBlank sets (5 questions each)
- 1 VerbConjugation set (5 questions)
- 1 SentenceCorrection set (5 questions)
- 1 SentenceOrdering set (3 questions)
- All A1 level English, published

**Acceptance Criteria:**
- [x] Schema is `"exercises"`
- [x] 6 tables + OutboxMessages configured
- [x] Unique constraint prevents duplicate answers per question
- [x] Seed provides testable exercises across MVP types

---

## Task 7.4: Exercises.Presentation — 6 Endpoints

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Exercises/EnglishTutor.Modules.Exercises.Presentation/
├── ExerciseEndpoints.cs
└── Requests/
    ├── GetExercisesRequest.cs
    └── SubmitAnswerRequest.cs
```

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/exercises` | Yes | List exercise sets (query: level, type, topic, skill) |
| GET | `/api/exercises/{id}` | Yes | Get exercise with questions (no correct answers) |
| POST | `/api/exercises/{id}/attempts` | Yes | Start new attempt |
| POST | `/api/exercises/attempts/{attemptId}/answers` | Yes | Submit answer for a question |
| POST | `/api/exercises/attempts/{attemptId}/complete` | Yes | Complete attempt, get score |
| GET | `/api/exercises/attempts/{attemptId}/result` | Yes | Get detailed result with all answers |

**Important:** `GET /api/exercises/{id}` must NOT return `CorrectAnswer` or `IsCorrect` on options — only after completion.

**Acceptance Criteria:**
- [x] Exercise detail hides correct answers
- [x] Result endpoint shows correct answers + explanations + user answers
- [x] Only attempt owner can submit answers / complete
- [x] POST returns 201 for start, 200 for submit/complete

---

## Task 7.5: Exercises.Contracts

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Exercises/EnglishTutor.Modules.Exercises.Contracts/
└── IntegrationEvents/
    ├── ExerciseStartedIntegrationEvent.cs
    ├── ExerciseQuestionAnsweredIntegrationEvent.cs
    └── ExerciseCompletedIntegrationEvent.cs
```

**ExerciseCompletedIntegrationEvent : IntegrationEvent:**
- `Guid UserId`, `Guid ExerciseSetId`, `Guid AttemptId`, `string TargetLanguageCode`, `string ExerciseType`, `int Score`, `int CorrectCount`, `int TotalQuestions`, `int TimeTakenSeconds`
- `List<WrongAnswerDetail> WrongAnswers`, `DateTime CompletedAtUtc`

**WrongAnswerDetail:**
- `Guid QuestionId`, `string Prompt`, `string UserAnswer`, `string CorrectAnswer`, `string? Explanation`, `string QuestionType`

**Acceptance Criteria:**
- [x] WrongAnswers list allows Mistakes module to create specific mistakes
- [x] Event carries all data needed — no callback required
- [x] All events are records

---

## Task 7.6: Wire Event Consumers

**Agent:** Integration & Events Agent

**Mistakes module — new handler:**

```
src/Modules/Mistakes/EnglishTutor.Modules.Mistakes.Application/EventHandlers/
    ExerciseCompletedEventHandler.cs
```

**ExerciseCompletedEventHandler flow:**
1. Check Inbox
2. For each `WrongAnswer` in event:
   - Determine mistake type from question type (Grammar, Vocabulary, etc.)
   - Create `Mistake.CreateFromCorrection(userId, SourceType=ExerciseAnswer, sourceId=questionId, original=userAnswer, corrected=correctAnswer, explanation)`
3. Save all mistakes
4. Save `MistakeCreatedIntegrationEvent` per mistake to outbox
5. Mark in Inbox

**Progress module — new handler:**

```
src/Modules/Progress/EnglishTutor.Modules.Progress.Application/EventHandlers/
    ExerciseCompletedEventHandler.cs
```

**Flow:**
1. Check Inbox
2. Create `LearningActivityLog` (type=ExerciseCompleted, score, duration)
3. Grant EXP: base 30 + (score/100 * 20) bonus
4. Update `UserSkillProgress` for Grammar + Vocabulary based on exercise type
5. Update `UserDailyProgress`, `UserDashboardSnapshot`, `UserStreak`
6. Mark in Inbox

**Acceptance Criteria:**
- [x] Mistakes created for each wrong answer
- [x] Progress EXP scales with score
- [x] Skill progress updated for relevant skills
- [x] Inbox prevents duplicate processing

---

## Task 7.7: Unit Tests

**Agent:** Test Writer

**Files to create:**

```
tests/EnglishTutor.Modules.Exercises.UnitTests/
├── Domain/
│   ├── ExerciseSetTests.cs
│   ├── UserExerciseAttemptTests.cs
│   └── ExerciseGradingTests.cs
├── Application/
│   ├── StartExerciseAttemptCommandHandlerTests.cs
│   ├── SubmitAnswerCommandHandlerTests.cs
│   └── CompleteExerciseCommandHandlerTests.cs
└── Services/
    └── ExerciseGradingServiceTests.cs
```

**Key test cases:**
- Attempt.Start creates with correct initial state
- RecordAnswer increments CorrectCount when correct
- RecordAnswer does not increment when incorrect
- Complete fails if not all questions answered
- Complete calculates correct score percentage
- Cannot answer same question twice
- Static grading: MultipleChoice matches IsCorrect option
- Static grading: FillInTheBlank case-insensitive comparison
- Static grading: SentenceOrdering word order match
- AI grading mock returns score + feedback
- Cannot submit answer to completed attempt

**Target:** At least 15 tests

---

## Task 7.8: Documentation

**Agent:** Documentation Agent

**Files to create:**

```
docs/api/exercises.md
docs/workflows/exercise-completion.md
```

**exercise-completion.md workflow:**
1. User browses exercises → selects set
2. Start attempt → API creates attempt record
3. User answers questions one by one → static or AI grading per question
4. User completes → score calculated → result saved
5. ExerciseCompletedEvent → Mistakes creates mistakes for wrong answers
6. ExerciseCompletedEvent → Progress updates EXP + skills + dashboard

---

## Task 7.9: Architecture Tests

**Agent:** Architecture Guardian

- Exercises must not reference AI.Infrastructure
- Exercises may reference AI.Contracts only
- Exercises must not reference Mistakes or Progress directly

---

## Phase 7 Definition of Done

- [x] 5 MVP exercise types work (MultipleChoice, FillInTheBlank, VerbConjugation, SentenceCorrection, SentenceOrdering)
- [x] Static grading correct for all MVP types
- [x] AI grading works for open-answer questions (mocked in tests)
- [x] Exercise detail hides correct answers until completion
- [x] Mistakes auto-created from wrong answers via events
- [x] Progress updated (EXP + skills) via events
- [x] `dotnet build && dotnet test` passes
- [x] At least 15 unit tests pass
- [x] API + workflow docs created
