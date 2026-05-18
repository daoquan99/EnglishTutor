# Phase 6: LearningContent + Conversations — Detailed Tasks

> **Goal:** Structured lessons, conversation scenarios, sentence patterns, learning path cards
> **Dependencies:** Phase 4 (Speaking module for conversation practice)
> **Estimated tasks:** 9

---

## Task 6.1: LearningContent.Domain

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/LearningContent/EnglishTutor.Modules.LearningContent.Domain/
├── Entities/
│   ├── Lesson.cs
│   ├── LessonTranslation.cs
│   ├── LessonSection.cs
│   ├── LessonSectionTranslation.cs
│   ├── ConversationScenario.cs
│   ├── ConversationScenarioTranslation.cs
│   ├── ConversationLine.cs
│   ├── ConversationLineTranslation.cs
│   ├── SentencePattern.cs
│   ├── Quiz.cs
│   └── UserLearningPathCard.cs
├── Enums/
│   ├── ContentType.cs
│   ├── LearningPathCardStatus.cs
│   └── ConversationSpeaker.cs
├── Events/
│   ├── LessonPublishedDomainEvent.cs
│   └── LessonCompletedDomainEvent.cs
└── Errors/
    └── LearningContentErrors.cs
```

**Lesson (AggregateRoot<Guid>):**
- `TargetLanguageCode (string)`, `Level (LanguageLevel)`, `Topic (string)`, `Skill (LearningSkill)`, `Title (string)`, `Description (string)`, `Order (int)`, `EstimatedMinutes (int)`, `IsPublished (bool)`, `CreatedAtUtc`, `UpdatedAtUtc`
- Navigation: `List<LessonSection>`, `List<LessonTranslation>`
- Method: `void Publish()` — validates has at least 1 section → sets IsPublished=true → raises `LessonPublishedDomainEvent`

**LessonTranslation (Entity<Guid>):**
- `LessonId`, `LanguageCode (string)`, `Title (string)`, `Description (string)`

**LessonSection (Entity<Guid>):**
- `LessonId`, `Title (string)`, `Content (string — markdown)`, `Order (int)`, `SectionType (string — Theory/Example/Practice)`
- Navigation: `List<LessonSectionTranslation>`

**LessonSectionTranslation (Entity<Guid>):**
- `LessonSectionId`, `LanguageCode`, `Title`, `Content`

**ConversationScenario (AggregateRoot<Guid>):**
- `TargetLanguageCode`, `Level`, `Title`, `Description`, `Setting (string — e.g. "coffee shop", "job interview")`, `Difficulty (int 1-5)`, `EstimatedMinutes (int)`, `IsPublished (bool)`, `CreatedAtUtc`
- Navigation: `List<ConversationLine>`, `List<ConversationScenarioTranslation>`

**ConversationScenarioTranslation (Entity<Guid>):**
- `ScenarioId`, `LanguageCode`, `Title`, `Description`, `Setting`

**ConversationLine (Entity<Guid>):**
- `ScenarioId (Guid)`, `Speaker (ConversationSpeaker)`, `Order (int)`, `Text (string)`, `ExpectedResponseHint (string?)`, `AudioUrl (string?)`, `Notes (string?)`
- Navigation: `List<ConversationLineTranslation>`

**ConversationSpeaker enum:** `User, AI`

**ConversationLineTranslation (Entity<Guid>):**
- `ConversationLineId`, `LanguageCode`, `Text`, `ExpectedResponseHint`

**SentencePattern (Entity<Guid>):**
- `TargetLanguageCode`, `Level`, `Pattern (string — e.g. "Subject + have/has + past participle")`, `Explanation (string)`, `Examples (string — JSON array)`, `Topic`, `CreatedAtUtc`

**UserLearningPathCard (Entity<Guid> — read model owned by this module):**
- `UserId (Guid)`, `TargetLanguageCode (string)`, `ContentType (ContentType)`, `ContentId (Guid)`, `Title (string)`, `Level (string)`, `Skill (string?)`, `Status (LearningPathCardStatus)`, `Order (int)`, `CompletedAtUtc (DateTime?)`, `LastUpdatedAtUtc`

**ContentType enum:** `Lesson, ConversationScenario, SentencePattern`
**LearningPathCardStatus enum:** `Locked, Available, InProgress, Completed`

**Acceptance Criteria:**
- [ ] Lesson requires at least 1 section to publish
- [ ] ConversationLines are ordered (Order field)
- [ ] All content entities support translations
- [ ] Learning path card is a read model (denormalized for fast query)
- [ ] No infrastructure dependencies

---

## Task 6.2: LearningContent.Application

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/LearningContent/EnglishTutor.Modules.LearningContent.Application/
├── Commands/
│   ├── CompleteLesson/
│   │   ├── CompleteLessonCommand.cs
│   │   └── CompleteLessonCommandHandler.cs
│   └── CompleteConversation/
│       ├── CompleteConversationCommand.cs
│       └── CompleteConversationCommandHandler.cs
├── Queries/
│   ├── GetLessons/
│   │   ├── GetLessonsQuery.cs
│   │   └── GetLessonsQueryHandler.cs
│   ├── GetLessonById/
│   │   ├── GetLessonByIdQuery.cs
│   │   └── GetLessonByIdQueryHandler.cs
│   ├── GetLearningPath/
│   │   ├── GetLearningPathQuery.cs
│   │   └── GetLearningPathQueryHandler.cs
│   ├── GetConversations/
│   │   ├── GetConversationsQuery.cs
│   │   └── GetConversationsQueryHandler.cs
│   └── GetConversationById/
│       ├── GetConversationByIdQuery.cs
│       └── GetConversationByIdQueryHandler.cs
├── EventHandlers/
│   ├── UserProfileUpdatedEventHandler.cs
│   ├── UserLevelChangedEventHandler.cs
│   └── SpeakingSessionCompletedEventHandler.cs
├── Abstractions/
│   ├── ILessonRepository.cs
│   ├── IConversationScenarioRepository.cs
│   └── ILearningPathCardRepository.cs
└── DTOs/
    ├── LessonListResponse.cs
    ├── LessonDetailResponse.cs
    ├── LearningPathCardResponse.cs
    ├── ConversationListResponse.cs
    └── ConversationDetailResponse.cs
```

**GetLessonsQuery:** `int Page`, `int PageSize`, `string? Level`, `string? Topic`, `string? Skill`, `string? TargetLanguageCode`
**Handler:** Query published lessons, include translations for user's UI language, return paginated

**GetLessonByIdQueryHandler flow:**
1. Load lesson with sections + translations
2. Filter translations to user's native/UI language
3. Return full lesson detail with translated content

**GetLearningPathQueryHandler flow:**
1. Get user's `TargetLanguageCode` from `ICurrentUser`
2. Query `UserLearningPathCards` ordered by `Order`
3. Return learning path with status per card

**CompleteLessonCommandHandler flow:**
1. Load lesson → validate exists + published
2. Update learning path card status → Completed + CompletedAtUtc
3. Unlock next card in path (next by Order)
4. Save `LessonCompletedIntegrationEvent` to outbox

**UserLevelChangedEventHandler flow:**
1. Check Inbox
2. Load user's learning path cards
3. Refresh: unlock cards matching new level, lock cards above new level
4. Generate new cards for content at the new level if not yet created
5. Mark in Inbox

**SpeakingSessionCompletedEventHandler flow:**
1. Check Inbox
2. If session was `ConversationPractice` type and has `ConversationScenarioId`
3. Find learning path card for that scenario → mark as Completed
4. Unlock next card
5. Mark in Inbox

**Acceptance Criteria:**
- [ ] Lesson list returns translations in user's language
- [ ] Learning path shows correct status per card
- [ ] Level change refreshes learning path
- [ ] Completing conversation via Speaking marks path card
- [ ] Inbox idempotency on all event handlers

---

## Task 6.3: LearningContent.Infrastructure

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/LearningContent/EnglishTutor.Modules.LearningContent.Infrastructure/
├── Persistence/
│   ├── LearningContentDbContext.cs
│   ├── Configurations/
│   │   ├── LessonConfiguration.cs
│   │   ├── LessonTranslationConfiguration.cs
│   │   ├── LessonSectionConfiguration.cs
│   │   ├── LessonSectionTranslationConfiguration.cs
│   │   ├── ConversationScenarioConfiguration.cs
│   │   ├── ConversationScenarioTranslationConfiguration.cs
│   │   ├── ConversationLineConfiguration.cs
│   │   ├── ConversationLineTranslationConfiguration.cs
│   │   ├── SentencePatternConfiguration.cs
│   │   ├── QuizConfiguration.cs
│   │   ├── UserLearningPathCardConfiguration.cs
│   │   ├── OutboxMessageConfiguration.cs
│   │   └── InboxMessageConfiguration.cs
│   ├── Repositories/
│   │   ├── LessonRepository.cs
│   │   ├── ConversationScenarioRepository.cs
│   │   └── LearningPathCardRepository.cs
│   └── Migrations/
├── ContractReaders/
│   └── ConversationScenarioReader.cs
├── DependencyInjection.cs
└── Seed/
    └── LearningContentSeedData.cs
```

**LearningContentDbContext:** Schema `"learningcontent"`, MigrationsHistoryTable `("__EFMigrationsHistory", "learningcontent")`

**EF Configurations:**
- `Lesson`: table `learningcontent.Lessons`, index on `(TargetLanguageCode, Level, IsPublished)`, index on `(Topic, Skill)`
- `LessonTranslation`: unique on `(LessonId, LanguageCode)`
- `LessonSection`: FK to Lesson, ordered by `Order`
- `ConversationScenario`: table `learningcontent.ConversationScenarios`, index on `(TargetLanguageCode, Level)`
- `ConversationLine`: FK to Scenario, ordered by `Order`
- `UserLearningPathCard`: index on `(UserId, TargetLanguageCode, Order)`, index on `(UserId, ContentType, ContentId)`

**ConversationScenarioReader : IConversationScenarioReader (from LearningContent.Contracts):**
- Returns `ConversationScenarioReadModel` with lines for Speaking module to use

**Seed data:**
- 5 lessons (A1 level) with 2-3 sections each, Vietnamese translations
- 3 conversation scenarios (coffee shop, introducing yourself, asking directions) with 5-8 lines each, Vietnamese translations
- 3 sentence patterns (present simple, past simple, present perfect)
- Initial learning path cards for seed content

**Acceptance Criteria:**
- [ ] Schema is `"learningcontent"`
- [ ] 11 tables + OutboxMessages + InboxMessages configured
- [ ] All translation tables have unique `(ParentId, LanguageCode)` constraint
- [ ] ConversationScenarioReader returns read model, not entity
- [ ] Seed data provides testable content

---

## Task 6.4: LearningContent.Presentation — 6 Endpoints

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/LearningContent/EnglishTutor.Modules.LearningContent.Presentation/
├── LessonEndpoints.cs
├── ConversationEndpoints.cs
├── LearningPathEndpoints.cs
└── Requests/
    └── GetLessonsRequest.cs
```

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/lessons` | Yes | List lessons (query: page, pageSize, level, topic, skill) |
| GET | `/api/lessons/{id}` | Yes | Get lesson with sections + translations |
| POST | `/api/lessons/{id}/complete` | Yes | Mark lesson as completed |
| GET | `/api/learning-path` | Yes | Get user's learning path cards |
| GET | `/api/conversations` | Yes | List conversation scenarios (query: level, difficulty) |
| GET | `/api/conversations/{id}` | Yes | Get scenario with conversation lines |

**Acceptance Criteria:**
- [ ] All 6 endpoints authenticated
- [ ] Lesson list supports level/topic/skill filtering
- [ ] Lesson detail returns translations in user's language
- [ ] Conversation detail returns lines in order with translations

---

## Task 6.5: LearningContent.Contracts

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/LearningContent/EnglishTutor.Modules.LearningContent.Contracts/
├── Readers/
│   └── IConversationScenarioReader.cs
├── ReadModels/
│   └── ConversationScenarioReadModel.cs
└── IntegrationEvents/
    ├── LessonPublishedIntegrationEvent.cs
    ├── LessonCompletedIntegrationEvent.cs
    └── ConversationScenarioCompletedIntegrationEvent.cs
```

**IConversationScenarioReader:**
```csharp
Task<ConversationScenarioReadModel?> GetByIdAsync(Guid scenarioId, CancellationToken ct);
```

**ConversationScenarioReadModel:**
- `Guid Id`, `string Title`, `string Setting`, `string Level`, `int Difficulty`, `List<ConversationLineReadModel> Lines`

**ConversationLineReadModel:**
- `int Order`, `string Speaker`, `string Text`, `string? ExpectedResponseHint`, `string? AudioUrl`

**LessonCompletedIntegrationEvent:**
- `Guid UserId`, `Guid LessonId`, `string TargetLanguageCode`, `string Level`, `string Topic`, `string Skill`, `int DurationSeconds`, `DateTime CompletedAtUtc`

**Acceptance Criteria:**
- [ ] Contract reader returns read model with lines
- [ ] Events carry enough data for Progress to log activity + grant EXP
- [ ] No domain entities exposed

---

## Task 6.6: Speaking Enhancement — Conversation Practice

**Agent:** Module Implementer

**Files to modify:**

```
src/Modules/Speaking/EnglishTutor.Modules.Speaking.Application/Commands/StartSession/
    StartSpeakingSessionCommand.cs (add ConversationScenarioId?)
    StartSpeakingSessionCommandHandler.cs (load scenario if conversation practice)

src/Modules/Speaking/EnglishTutor.Modules.Speaking.Domain/Entities/
    SpeakingSession.cs (add ConversationScenarioId field)
```

**Updated StartSpeakingSessionCommandHandler flow:**
1. If `SessionType == ConversationPractice` and `ConversationScenarioId` provided:
   - Call `LearningContent.Contracts.IConversationScenarioReader.GetByIdAsync(scenarioId)`
   - Validate scenario exists
   - Store `ConversationScenarioId` on session
   - Use scenario lines to guide conversation turns
2. Rest of flow unchanged (language snapshot, save session)

**SpeakingSession changes:**
- Add `ConversationScenarioId (Guid?)` field
- Add `CurrentLineOrder (int?)` to track conversation progress

**Acceptance Criteria:**
- [ ] Conversation practice loads scenario from LearningContent.Contracts
- [ ] Session stores scenario reference
- [ ] Speaking does NOT reference LearningContent.Infrastructure
- [ ] Free talk sessions still work without scenario

---

## Task 6.7: Event Wiring

**Agent:** Integration & Events Agent

Wire consumers:
- `LessonCompletedIntegrationEvent` → `Progress.LessonCompletedEventHandler` (log activity, grant EXP)
- `ConversationScenarioCompletedIntegrationEvent` → `Progress.ConversationCompletedEventHandler`
- `UserLevelChangedIntegrationEvent` → `LearningContent.UserLevelChangedEventHandler` (refresh path)
- `SpeakingSessionCompletedIntegrationEvent` → `LearningContent.SpeakingSessionCompletedEventHandler` (mark conversation completed)

Add `OutboxMessages` + `InboxMessages` to LearningContentDbContext.

---

## Task 6.8: Tests

**Agent:** Test Writer

**Files to create:**

```
tests/EnglishTutor.Modules.LearningContent.UnitTests/
├── Domain/
│   ├── LessonTests.cs
│   ├── ConversationScenarioTests.cs
│   └── UserLearningPathCardTests.cs
└── Application/
    ├── CompleteLessonCommandHandlerTests.cs
    ├── GetLearningPathQueryHandlerTests.cs
    └── UserLevelChangedEventHandlerTests.cs
```

**Key test cases:**
- Lesson.Publish() fails without sections
- CompleteLessonHandler updates path card status + unlocks next
- Learning path sorted by Order
- UserLevelChangedHandler refreshes cards for new level
- ConversationLine ordering is correct
- GetLessonById returns translations for specified language

**Target:** At least 12 tests

---

## Task 6.9: Documentation

**Agent:** Documentation Agent

**Files to create:**

```
docs/api/learning-content.md
```

**Content:** 6 endpoints with request/response JSON, validation, error codes, application flow, related events

---

## Phase 6 Definition of Done

- [ ] Lessons with sections + translations queryable with filters
- [ ] Conversation scenarios with ordered lines + translations
- [ ] Learning path cards show correct status per user
- [ ] Lesson completion updates path card + unlocks next
- [ ] Level change refreshes learning path via events
- [ ] Conversation practice in Speaking loads scenario from LearningContent.Contracts
- [ ] `dotnet build && dotnet test` passes
- [ ] At least 12 unit tests pass
- [ ] API docs created
