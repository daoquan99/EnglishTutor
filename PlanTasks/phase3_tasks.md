# Phase 3: AI Module + Vocabulary Core — Detailed Tasks

> **Goal:** AI infrastructure (Gemini/Gemma clients, model routing, prompt templates) + Vocabulary flashcard system (items, translations, examples, mastery, pronunciation)
> **Dependencies:** Phase 2 (Users.Contracts for user language context)
> **Estimated tasks:** 13

> **Architecture correction:** Any module error catalog such as `AiErrors` or `VocabularyErrors` must live in the module Application layer (`*.Application/Errors`) and use `EnglishTutor.BuildingBlocks.Application.Results.Error`. Domain must not contain `Errors/*Errors.cs` catalogs or reference Application `Result/Error`; Domain invariant failures use Domain exceptions/business rules.

---

## Task 3.1: AI.Domain — Core Entities

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AI/EnglishTutor.Modules.AI.Domain/
├── Entities/
│   ├── AiRequestLog.cs
│   ├── PromptTemplate.cs
│   ├── PromptVersion.cs
│   ├── ModelRoutingRule.cs
│   ├── AiUsageCounter.cs
│   └── AiCostEstimation.cs
├── Enums/
│   ├── AiTaskType.cs
│   ├── AiModelType.cs
│   └── AiRequestStatus.cs
└── Errors/
    └── AiErrors.cs
```

**AiTaskType enum:**
```
SentenceCorrection, ExerciseGeneration, VocabularyExampleGeneration,
WritingCorrection, GrammarExplanation, AssessmentGrading,
RealtimeVoice, TextToSpeech, DailyPlanGeneration
```

**AiModelType enum:**
```
Gemma, GeminiFlash, GeminiPro, GeminiLive, GeminiTTS
```

**AiRequestLog (AggregateRoot):**
- `UserId`, `TaskType`, `ModelUsed`, `PromptTokens`, `CompletionTokens`, `TotalTokens`, `LatencyMs`, `Status (Success/Failed/Timeout)`, `RequestPayloadHash`, `ErrorMessage`, `CreatedAtUtc`

**PromptTemplate (AggregateRoot):**
- `Name` (unique), `TaskType`, `Description`, `IsActive`, `CreatedAtUtc`
- One-to-many with `PromptVersion`

**PromptVersion (Entity):**
- `PromptTemplateId`, `VersionNumber`, `SystemPrompt`, `UserPromptTemplate`, `IsActive`, `CreatedAtUtc`
- Only one version active per template

**ModelRoutingRule (Entity):**
- `TaskType`, `PreferredModel`, `FallbackModel`, `MaxTokens`, `Temperature`, `IsActive`

**Acceptance Criteria:**
- [x] AiRequestLog captures all metrics for cost/usage analysis
- [x] PromptTemplate supports versioning
- [x] ModelRoutingRule maps task types to AI models
- [x] No external AI client dependencies in Domain

---

## Task 3.2: AI.Application — Correction & Generation Handlers

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AI/EnglishTutor.Modules.AI.Application/
├── Commands/
│   ├── CorrectSentence/
│   │   ├── CorrectSentenceCommand.cs
│   │   └── CorrectSentenceCommandHandler.cs
│   └── GenerateVocabularyExamples/
│       ├── GenerateVocabularyExamplesCommand.cs
│       └── GenerateVocabularyExamplesCommandHandler.cs
├── Abstractions/
│   ├── IAiClient.cs
│   ├── IModelRouter.cs
│   ├── IPromptBuilder.cs
│   ├── IAiRequestLogRepository.cs
│   ├── IPromptTemplateRepository.cs
│   └── IModelRoutingRuleRepository.cs
└── DTOs/
    ├── SentenceCorrectionResult.cs
    ├── AiLanguageContext.cs
    └── VocabularyExampleResult.cs
```

**IAiClient (abstraction):**
```csharp
Task<AiResponse> SendAsync(AiRequest request, CancellationToken ct);
```

**IModelRouter:**
```csharp
AiModelType ResolveModel(AiTaskType taskType);
ModelRoutingRule GetRoutingRule(AiTaskType taskType);
```

**IPromptBuilder:**
```csharp
string BuildPrompt(string templateName, Dictionary<string, string> variables);
```

**AiLanguageContext (shared DTO):**
```csharp
public sealed record AiLanguageContext(
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    string UserLevel,
    string? Topic);
```

**CorrectSentenceCommand:**
- `string OriginalText`, `AiLanguageContext LanguageContext`
- Handler: resolve model → build prompt → call AI → log request → return result

**SentenceCorrectionResult:**
- `string OriginalText`, `string CorrectedText`, `string NaturalVersion`, `int GrammarScore`, `int VocabularyScore`, `string Feedback`, `List<MistakeDetail> Mistakes`

**Acceptance Criteria:**
- [x] All AI calls go through `IAiClient` abstraction
- [x] Model routing resolved per task type
- [x] Every AI call logged to `AiRequestLog`
- [x] Language context included in all prompts
- [x] CancellationToken on all async methods

---

## Task 3.3: AI.Infrastructure — Gemini/Gemma Clients & Routing

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AI/EnglishTutor.Modules.AI.Infrastructure/
├── Persistence/
│   ├── AiDbContext.cs
│   ├── Configurations/
│   │   ├── AiRequestLogConfiguration.cs
│   │   ├── PromptTemplateConfiguration.cs
│   │   ├── PromptVersionConfiguration.cs
│   │   ├── ModelRoutingRuleConfiguration.cs
│   │   ├── AiUsageCounterConfiguration.cs
│   │   └── AiCostEstimationConfiguration.cs
│   ├── Repositories/
│   │   ├── AiRequestLogRepository.cs
│   │   ├── PromptTemplateRepository.cs
│   │   └── ModelRoutingRuleRepository.cs
│   └── Migrations/
├── Clients/
│   ├── GeminiClient.cs
│   ├── GemmaClient.cs
│   ├── AiClientFactory.cs
│   └── AiClientOptions.cs
├── Routing/
│   └── ModelRouter.cs
├── Prompts/
│   ├── PromptBuilder.cs
│   └── Templates/
│       ├── sentence_correction_v1.txt
│       └── vocabulary_examples_v1.txt
├── ContractImplementations/
│   └── EnglishCorrectionService.cs
├── DependencyInjection.cs
└── Seed/
    └── AiSeedData.cs
```

**GeminiClient / GemmaClient:**
- HttpClient-based
- Call Google AI API (Gemini) or local/cloud Gemma endpoint
- Parse response JSON → `AiResponse`
- Handle rate limits, timeouts, retries

**ModelRouter : IModelRouter:**
- Load `ModelRoutingRules` from DB (cached)
- Map `AiTaskType` → `AiModelType`
- Fallback model if primary fails

**AiClientFactory:**
- Resolve correct `IAiClient` implementation based on `AiModelType`

**EnglishCorrectionService : IEnglishCorrectionService (from AI.Contracts):**
- Orchestrates: get user language context → build prompt → route model → call AI → log → return result
- This is the contract implementation used by Speaking, Exercises, etc.

**Seed data:**
- Default `ModelRoutingRules` for each `AiTaskType`
- Default `PromptTemplate` + `PromptVersion` for sentence correction

**Acceptance Criteria:**
- [x] AI clients only exist in AI.Infrastructure (architecture test enforced)
- [x] Gemini/Gemma API keys from configuration, never hardcoded
- [x] All AI requests logged with token counts and latency
- [x] Model routing is data-driven (DB), not hardcoded
- [x] Schema is `"ai"`

---

## Task 3.4: AI.Contracts — Public Interfaces

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/AI/EnglishTutor.Modules.AI.Contracts/
├── Services/
│   ├── IEnglishCorrectionService.cs
│   ├── IAiExerciseGenerator.cs
│   ├── IAiLessonGenerator.cs
│   ├── IAssessmentGradingService.cs
│   └── IAudioGenerationService.cs
└── DTOs/
    ├── CorrectionRequest.cs
    ├── CorrectionResponse.cs
    ├── MistakeDetail.cs
    ├── ExerciseGenerationRequest.cs
    ├── ExerciseGenerationResponse.cs
    ├── GradingRequest.cs
    └── GradingResponse.cs
```

**IEnglishCorrectionService:**
```csharp
Task<CorrectionResponse> CorrectSentenceAsync(CorrectionRequest request, CancellationToken ct);
```

**CorrectionRequest:**
- `string OriginalText`, `string NativeLanguageCode`, `string TargetLanguageCode`, `string ExplanationLanguageCode`, `string UserLevel`, `string? Topic`

**CorrectionResponse:**
- `string CorrectedText`, `string NaturalVersion`, `int GrammarScore`, `int VocabularyScore`, `string Feedback`, `string FeedbackLanguageCode`, `List<MistakeDetail> Mistakes`

**MistakeDetail:**
- `string Type` (Grammar/Vocabulary/Spelling), `string Original`, `string Corrected`, `string Explanation`

**Acceptance Criteria:**
- [x] Only interfaces and DTOs — no implementations
- [x] No reference to AI.Domain or AI.Infrastructure
- [x] DTOs are records (immutable)
- [x] All methods use CancellationToken

---

## Task 3.5: Vocabulary.Domain — Flashcard Entities

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Domain/
├── Entities/
│   ├── VocabularyItem.cs
│   ├── VocabularyTranslation.cs
│   ├── VocabularyExample.cs
│   ├── VocabularyExampleTranslation.cs
│   ├── UserVocabularyMastery.cs
│   ├── VocabularyReviewSession.cs
│   ├── VocabularyReviewAttempt.cs
│   ├── VocabularyPronunciationAttempt.cs
│   └── ExampleSentencePronunciationAttempt.cs
├── Enums/
│   ├── VocabularyMasteryStatus.cs
│   └── PartOfSpeech.cs
├── Events/
│   ├── VocabularyReviewedDomainEvent.cs
│   ├── VocabularyMasteredDomainEvent.cs
│   ├── VocabularyPronunciationPracticedDomainEvent.cs
│   └── ExampleSentencePronunciationPracticedDomainEvent.cs
└── Errors/
    └── VocabularyErrors.cs
```

**VocabularyItem (AggregateRoot):**
- `TargetLanguageCode`, `Word`, `Phonetic`, `Level`, `Topic`, `PartOfSpeech`, `CreatedAtUtc`
- Navigation: `List<VocabularyTranslation>`, `List<VocabularyExample>`

**VocabularyMasteryStatus enum:** `New, Learning, Reviewing, Weak, Mastered`

**UserVocabularyMastery (Entity):**
- `UserId`, `VocabularyItemId`, `TargetLanguageCode`
- `MeaningMasteryScore`, `PronunciationMasteryScore`, `ExampleSentenceScore`
- `ReviewCount`, `CorrectReviewCount`, `LastReviewedAtUtc`, `NextReviewAtUtc`, `Status`
- Method: `void RecordReview(bool isCorrect, int score)` — updates scores, calculates next review (spaced repetition)
- Method: `void MarkMastered()` — raises `VocabularyMasteredDomainEvent`

**VocabularyPronunciationAttempt (Entity):**
- `UserId`, `VocabularyItemId`, `TargetLanguageCode`, `AudioUrl`, `RecognizedText`
- `PronunciationScore`, `AccuracyScore`, `FluencyScore`, `CompletenessScore`, `Feedback`, `AttemptedAtUtc`

**Acceptance Criteria:**
- [x] Spaced repetition logic in domain (NextReviewAtUtc calculation)
- [x] Mastery status transitions: New→Learning→Reviewing→Mastered (or →Weak)
- [x] Domain events raised on review and mastery
- [x] Pronunciation attempts store all scoring metrics

---

## Task 3.6: Vocabulary.Application — Study Flow Handlers

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Application/
├── Commands/
│   ├── ReviewVocabulary/
│   │   ├── ReviewVocabularyCommand.cs
│   │   ├── ReviewVocabularyCommandHandler.cs
│   │   └── ReviewVocabularyCommandValidator.cs
│   ├── SubmitPronunciationAttempt/
│   │   ├── SubmitPronunciationAttemptCommand.cs
│   │   └── SubmitPronunciationAttemptCommandHandler.cs
│   ├── SubmitExamplePronunciation/
│   │   ├── SubmitExamplePronunciationCommand.cs
│   │   └── SubmitExamplePronunciationCommandHandler.cs
│   └── MarkMastered/
│       ├── MarkMasteredCommand.cs
│       └── MarkMasteredCommandHandler.cs
├── Queries/
│   ├── GetTodayVocabulary/
│   │   ├── GetTodayVocabularyQuery.cs
│   │   └── GetTodayVocabularyQueryHandler.cs
│   └── GetStudyCard/
│       ├── GetStudyCardQuery.cs
│       └── GetStudyCardQueryHandler.cs
├── Abstractions/
│   ├── IVocabularyItemRepository.cs
│   ├── IUserVocabularyMasteryRepository.cs
│   ├── IVocabularyReviewRepository.cs
│   └── IPronunciationAttemptRepository.cs
└── DTOs/
    ├── TodayVocabularyResponse.cs
    ├── StudyCardResponse.cs
    └── ReviewResultResponse.cs
```

**GetTodayVocabularyQueryHandler:**
1. Get user's active target language from `ICurrentUser`
2. Query vocabulary items due for review (`NextReviewAtUtc <= UtcNow`) + new items
3. Return list with mastery status per item

**GetStudyCardQueryHandler:**
1. Load `VocabularyItem` with translations (in user's native language) and examples
2. Load `UserVocabularyMastery` for scores
3. Return full study card DTO

**ReviewVocabularyCommandHandler:**
1. Load mastery record (or create if first review)
2. Call `mastery.RecordReview(isCorrect, score)`
3. Save attempt to `VocabularyReviewAttempt`
4. Save integration event to outbox

**Acceptance Criteria:**
- [x] Today's vocabulary considers spaced repetition schedule
- [x] Study card includes translations in user's native language
- [x] Review updates mastery scores and next review date
- [x] All commands save outbox messages

---

## Task 3.7: Vocabulary.Infrastructure — DbContext & Migrations

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Infrastructure/
├── Persistence/
│   ├── VocabularyDbContext.cs
│   ├── Configurations/
│   │   ├── VocabularyItemConfiguration.cs
│   │   ├── VocabularyTranslationConfiguration.cs
│   │   ├── VocabularyExampleConfiguration.cs
│   │   ├── VocabularyExampleTranslationConfiguration.cs
│   │   ├── UserVocabularyMasteryConfiguration.cs
│   │   ├── VocabularyReviewSessionConfiguration.cs
│   │   ├── VocabularyReviewAttemptConfiguration.cs
│   │   ├── VocabularyPronunciationAttemptConfiguration.cs
│   │   ├── ExampleSentencePronunciationAttemptConfiguration.cs
│   │   └── OutboxMessageConfiguration.cs
│   ├── Repositories/
│   └── Migrations/
├── DependencyInjection.cs
└── Seed/
    └── VocabularySeedData.cs
```

**VocabularyDbContext:** Schema `"vocabulary"`, MigrationsHistoryTable in `"vocabulary"` schema

**Key indexes:**
- `VocabularyItem`: unique on `(TargetLanguageCode, Word, PartOfSpeech)`
- `UserVocabularyMastery`: unique on `(UserId, VocabularyItemId, TargetLanguageCode)`, index on `(UserId, NextReviewAtUtc)`
- `VocabularyPronunciationAttempt`: index on `(UserId, VocabularyItemId)`

**Seed data:** 20-30 sample vocabulary items for English (common words) with Vietnamese translations

**Acceptance Criteria:**
- [x] Schema is `"vocabulary"`
- [x] All 10 tables configured
- [x] Performance indexes for query patterns
- [x] Seed data provides testable vocabulary

---

## Task 3.8: Vocabulary.Presentation — API Endpoints

**Agent:** Module Implementer

**Endpoints:**

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/vocabulary/today` | Yes | Get today's vocabulary for review |
| GET | `/api/vocabulary/{id}/study-card` | Yes | Get full study card |
| POST | `/api/vocabulary/{id}/review` | Yes | Submit review result |
| POST | `/api/vocabulary/{id}/pronunciation-attempts` | Yes | Submit pronunciation attempt |
| POST | `/api/vocabulary/examples/{exampleId}/pronunciation-attempts` | Yes | Submit example sentence pronunciation |
| POST | `/api/vocabulary/{id}/mark-mastered` | Yes | Mark vocabulary as mastered |

**Acceptance Criteria:**
- [x] 6 endpoints, all authenticated
- [x] Proper route parameter naming
- [x] POST endpoints return 201
- [x] GET endpoints return 200

---

## Task 3.9: Vocabulary.Contracts — Events & Read Models

**Agent:** Module Implementer

**Files to create:**

```
src/Modules/Vocabulary/EnglishTutor.Modules.Vocabulary.Contracts/
└── IntegrationEvents/
    ├── VocabularyReviewedIntegrationEvent.cs
    ├── VocabularyMasteredIntegrationEvent.cs
    ├── VocabularyPronunciationPracticedIntegrationEvent.cs
    └── ExampleSentencePronunciationPracticedIntegrationEvent.cs
```

**VocabularyReviewedIntegrationEvent:**
- `Guid UserId`, `Guid VocabularyItemId`, `string TargetLanguageCode`, `bool IsCorrect`, `int Score`, `string MasteryStatus`, `DateTime ReviewedAtUtc`

**VocabularyPronunciationPracticedIntegrationEvent:**
- `Guid UserId`, `Guid VocabularyItemId`, `string TargetLanguageCode`, `int PronunciationScore`, `int AccuracyScore`, `int FluencyScore`, `DateTime PracticedAtUtc`

**Acceptance Criteria:**
- [x] All events are past tense
- [x] Events carry enough data for consumers (Progress, Mistakes) to act without callback
- [x] No domain entities in contracts

---

## Task 3.10: Vocabulary Outbox Integration

**Agent:** Integration & Events Agent

**Implementation:**
- Add `OutboxMessages` table to `vocabulary` schema
- Map domain events → integration events in `VocabularyDomainEventToOutboxMapper`
- Wire into `VocabularyDbContext.SaveChangesAsync` interceptor
- Register outbox table in Worker's `IOutboxProcessor` scan list

**Acceptance Criteria:**
- [x] Vocabulary review → outbox message created in same transaction
- [x] Worker processes vocabulary outbox messages
- [x] Events deserialized and dispatched correctly

---

## Task 3.11: AI + Vocabulary Unit Tests

**Agent:** Test Writer

**Files to create:**

```
tests/EnglishTutor.Modules.AI.UnitTests/
├── Domain/ModelRoutingRuleTests.cs
├── Application/CorrectSentenceCommandHandlerTests.cs
└── Routing/ModelRouterTests.cs

tests/EnglishTutor.Modules.Vocabulary.UnitTests/
├── Domain/
│   ├── UserVocabularyMasteryTests.cs
│   ├── VocabularyItemTests.cs
│   └── SpacedRepetitionTests.cs
├── Application/
│   ├── ReviewVocabularyCommandHandlerTests.cs
│   └── GetTodayVocabularyQueryHandlerTests.cs
└── Validators/
    └── ReviewVocabularyCommandValidatorTests.cs
```

**Key test cases:**
- ModelRouter resolves correct model per task type
- CorrectSentenceHandler logs AI request
- Mastery score updates correctly after review
- Spaced repetition calculates correct NextReviewAtUtc
- Mastery status transitions (New→Learning→Reviewing→Mastered)
- GetTodayVocabulary returns items due for review

**Acceptance Criteria:**
- [x] At least 15 tests across AI + Vocabulary
- [x] Spaced repetition logic thoroughly tested
- [x] AI handler tests mock IAiClient

---

## Task 3.12: Architecture Tests — AI Boundary Enforcement

**Agent:** Architecture Guardian

**New rules to add:**
- AI client classes (`GeminiClient`, `GemmaClient`) must exist only in `AI.Infrastructure`
- No module except AI may reference `Google.AI` or Gemini SDK namespaces
- Vocabulary must not reference AI.Infrastructure
- Vocabulary may reference AI.Contracts only

**Acceptance Criteria:**
- [x] Test fails if Speaking/Exercises/Assessments imports Gemini client
- [x] Test fails if AI client class found outside AI.Infrastructure

---

## Task 3.13: API Documentation — AI & Vocabulary

**Agent:** Documentation Agent

**Files to create:**

```
docs/api/ai.md
docs/api/vocabulary.md
```

**Acceptance Criteria:**
- [x] AI docs note which endpoints are internal vs public
- [x] Vocabulary docs include spaced repetition behavior description
- [x] All 6 vocabulary endpoints + AI endpoints documented
- [x] Integration events listed with payload fields

---

## Phase 3 Definition of Done

- [x] AI sentence correction works end-to-end (API → AI client → response)
- [x] AI request logged with token counts and latency
- [x] Vocabulary CRUD + review + mastery tracking works
- [x] Spaced repetition calculates next review dates
- [x] Pronunciation attempts stored with scores
- [x] Outbox messages created for vocabulary events
- [x] Architecture tests enforce AI client isolation
- [x] `dotnet build && dotnet test` passes
- [x] Docs updated
