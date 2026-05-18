# EnglishTutor Backend Architecture Document

## 1. Project Overview

**Project name:** EnglishTutor

**Goal:** Build a production-grade backend for an AI-powered English learning application. The first target user is a Vietnamese C# developer learning English, but the backend must be designed for multi-language learning in the future.

The system should support:

* Multi-language learning: Native Language, UI Language, Explanation Language, Target Language
* User authentication and user learning profile
* Study planning and scheduled learning reminders
* Learning content: lessons, conversations, sentence patterns, quizzes/drills content
* Vocabulary flashcards with example sentences
* Detailed vocabulary attempts, pronunciation attempts, and mastery scores
* Exercises: multiple choice, fill in the blank, verb conjugation, sentence correction, sentence ordering, translation, matching, etc.
* Speaking practice sessions and conversation practice runtime
* AI-powered correction, grading, model routing, prompt templates, and usage logs
* Mistake tracking and mistake review
* Assessments: placement tests, level-up tests, strict AI-assisted grading
* Progress tracking: EXP, streak, skill progress, daily/weekly/monthly progress, dashboard snapshots
* Notifications: study reminders, missed-study reminders, review reminders, weekly/monthly summaries
* Admin reports and analytics
* Horizontal scaling with API instances and Worker host
* Reliable internal events using Outbox/Inbox

---

## 2. Architecture Decision

### Selected architecture

```text
Production Modular Monolith
+ Clean Architecture per Module
+ DDD tactical patterns per Module
+ DbContext per Module
+ Schema per Module
+ Contracts for synchronous cross-module reads
+ Integration Events for state changes
+ Transactional Outbox per producer module
+ Inbox per consumer module for idempotency
+ Retry + Dead-letter for event failures
+ Read Models / Projections for aggregate screens
+ API Host + Worker Host
+ Architecture Tests to enforce boundaries
```

### Why Modular Monolith?

The system has many business capabilities, but it should not start as microservices. Microservices would add unnecessary operational complexity: service discovery, distributed transactions, broker operations, distributed tracing, and service versioning.

A production modular monolith gives the project:

* Strong module boundaries
* One deployable backend system
* Simpler operations than microservices
* Clear data ownership per module
* Easier horizontal scaling than a tightly coupled monolith
* A clean path to extract modules later if truly needed

### Long-term direction

The main architecture remains **Modular Monolith**. Microservices are only a future option if scale, organization, or operational needs require it.

---

## 3. Module Map

The current production module list:

```text
Core Identity/User
1. Auth
2. Users

Planning/Engagement
3. StudyPlans
4. Notifications

Learning Domain
5. LearningContent
6. Vocabulary
7. Exercises
8. Speaking
9. Mistakes
10. Assessments

Intelligence & Analytics
11. AI
12. Progress
13. AdminReports
```

### Why these modules?

Modules are divided by **business capability / bounded context**, not by technical layers or CRUD tables.

Each module should own:

```text
- Domain model
- Application use cases
- DbContext
- Database schema
- Migrations
- Contracts
- Integration events
- Read models owned by that module
```

---

## 4. Core Architecture Rules

These rules are mandatory.

```text
1. Each module owns its own DbContext.
2. Each module owns its own database schema.
3. Each module owns its own migrations.
4. A module must not access another module's DbContext.
5. A module must not reference another module's Domain/Application/Infrastructure/Presentation.
6. A module may reference only another module's Contracts project.
7. Do not expose IQueryable across module boundaries.
8. Do not expose Domain Entities through Contracts.
9. Do not use cross-module navigation properties.
10. Do not perform cross-module EF joins in core business logic.
11. Use Contract Readers for small synchronous reads.
12. Use Integration Events for state changes.
13. Use Read Models / Projections for dashboards and aggregate screens.
14. Use Database Views only for admin/reporting, not core business flow.
15. AI provider clients must exist only inside the AI module.
16. Other modules must use AI.Contracts only.
17. All important integration events should use Outbox/Inbox.
18. Event handlers must be idempotent.
19. API must be stateless to support horizontal scaling.
20. Architecture tests must enforce module boundaries.
```

---

## 5. Solution Structure

Recommended production structure:

```text
src
├── Bootstrapper
│   ├── EnglishTutor.Api
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── DependencyInjection.cs
│   │   ├── Middlewares
│   │   └── OpenApi
│   │
│   └── EnglishTutor.Worker
│       ├── Program.cs
│       ├── appsettings.json
│       ├── DependencyInjection.cs
│       └── Jobs
│
├── BuildingBlocks
│   ├── EnglishTutor.BuildingBlocks.Domain
│   ├── EnglishTutor.BuildingBlocks.Application
│   ├── EnglishTutor.BuildingBlocks.Infrastructure
│   ├── EnglishTutor.BuildingBlocks.EventBus
│   ├── EnglishTutor.BuildingBlocks.Outbox
│   └── EnglishTutor.BuildingBlocks.SharedKernel
│
├── Modules
│   ├── Auth
│   │   ├── EnglishTutor.Modules.Auth.Domain
│   │   ├── EnglishTutor.Modules.Auth.Application
│   │   ├── EnglishTutor.Modules.Auth.Infrastructure
│   │   ├── EnglishTutor.Modules.Auth.Presentation
│   │   └── EnglishTutor.Modules.Auth.Contracts
│   │
│   ├── Users
│   ├── StudyPlans
│   ├── LearningContent
│   ├── Vocabulary
│   ├── Exercises
│   ├── Speaking
│   ├── AI
│   ├── Mistakes
│   ├── Assessments
│   ├── Progress
│   ├── Notifications
│   └── AdminReports
│
└── Tests
    ├── EnglishTutor.ArchitectureTests
    ├── EnglishTutor.IntegrationTests
    ├── EnglishTutor.Modules.Auth.UnitTests
    ├── EnglishTutor.Modules.Users.UnitTests
    ├── EnglishTutor.Modules.Speaking.UnitTests
    ├── EnglishTutor.Modules.Vocabulary.UnitTests
    ├── EnglishTutor.Modules.Exercises.UnitTests
    └── EnglishTutor.Modules.AI.UnitTests
```

---

## 6. Runtime Deployment Topology

Recommended topology:

```text
Next.js Frontend
      ↓
Load Balancer
      ↓
EnglishTutor.Api x N
      ↓
Shared Database
Redis
Object Storage
Observability Stack

EnglishTutor.Worker x 1 initially
EnglishTutor.Worker x N later if needed
```

### EnglishTutor.Api

Responsibilities:

```text
- HTTP APIs
- Authentication and authorization
- Request validation
- Command/query execution
- Write business data
- Write OutboxMessages
- Stateless horizontal scaling
```

### EnglishTutor.Worker

Responsibilities:

```text
- Outbox processing
- Integration event dispatching
- Notification scheduling
- Missed study session detection
- Weekly/monthly report generation
- Async AI jobs
- Projection rebuild jobs
- Inbox cleanup jobs
```

`EnglishTutor.Worker` is not a microservice. It is a background host in the same modular monolith solution.

---

## 7. Horizontal Scaling Strategy

The API must be stateless.

When scaling:

```text
Load Balancer
  ├── EnglishTutor.Api instance 1
  ├── EnglishTutor.Api instance 2
  ├── EnglishTutor.Api instance 3
  └── EnglishTutor.Api instance N
```

API instances do not dispatch events directly in memory during requests. They only write OutboxMessages into the database.

Flow:

```text
1. Request goes to any API instance.
2. API processes command.
3. API saves business data.
4. API saves IntegrationEvent as OutboxMessage in same transaction.
5. API returns response.
6. Worker reads OutboxMessages.
7. Worker dispatches event to internal handlers.
```

This keeps event delivery independent of which API instance handled the request.

### Worker scaling

Start with:

```text
EnglishTutor.Worker x 1
```

Later:

```text
EnglishTutor.Worker x N
```

If Worker is scaled horizontally, Outbox processing must use DB row locking/leasing.

Outbox fields should include:

```text
Status
LockedBy
LockedUntilUtc
RetryCount
NextRetryAtUtc
```

Event delivery model:

```text
At-least-once delivery
+ Idempotent handlers through Inbox
```

Do not try to guarantee exactly-once delivery at infrastructure level.

---

## 8. BuildingBlocks

`BuildingBlocks` contains shared technical and architectural foundations. It must not contain module-specific business logic.

### 8.1 Domain

Project:

```text
EnglishTutor.BuildingBlocks.Domain
```

Contains:

```text
Entity
AggregateRoot
ValueObject
DomainEvent
DomainException
BusinessRuleValidationException
```

### 8.2 Application

Project:

```text
EnglishTutor.BuildingBlocks.Application
```

Contains:

```text
Result<T>
Error
PagedResult<T>
IDateTimeProvider
ICurrentUser
Command/Query abstractions
Validation abstractions
Pipeline behaviors
```

### 8.3 Infrastructure

Project:

```text
EnglishTutor.BuildingBlocks.Infrastructure
```

Contains:

```text
DateTimeProvider
JsonSerializerService
FileStorage abstractions/implementations
Common persistence helpers
Common middleware helpers
```

### 8.4 EventBus

Project:

```text
EnglishTutor.BuildingBlocks.EventBus
```

Contains:

```text
IEventBus
IIntegrationEvent
IntegrationEvent
IIntegrationEventHandler<T>
InProcessEventBus
```

RabbitMQ is not needed at the beginning, but the abstraction should allow a RabbitMQ adapter later.

### 8.5 Outbox

Project:

```text
EnglishTutor.BuildingBlocks.Outbox
```

Contains:

```text
OutboxMessage
InboxMessage
DeadLetterMessage
IOutboxProcessor
OutboxOptions
OutboxBackgroundJob
Retry policy helpers
```

### 8.6 SharedKernel

Project:

```text
EnglishTutor.BuildingBlocks.SharedKernel
```

Contains only tiny shared concepts:

```text
LanguageCode
LanguageLevel
LearningSkill
UserId
Maybe Email value object if truly shared
```

Do not put feature-specific business logic here.

---

## 9. Multi-Language Design

### 9.1 Language is not Tenant

Language should not be modeled as tenant.

Use:

```text
1 shared system
1 physical database
schemas per module
language code columns in user/content/progress/AI context
```

Do not use multi-tenancy for language.

Multi-tenancy should only be added later if the product supports organizations such as schools, companies, teachers, or classes.

```text
Tenant = organization/customer boundary
Language = user/content preference
```

### 9.2 User language concepts

Each user has:

```text
NativeLanguageCode       = user's mother language
UiLanguageCode           = FE display language
ExplanationLanguageCode  = language used for explanations
ActiveTargetLanguageCode = language currently being learned
```

Future support:

```text
One user can learn multiple target languages.
```

### 9.3 Recommended Users tables

```text
users.UserLanguageSettings
- Id
- UserId
- NativeLanguageCode
- UiLanguageCode
- ExplanationLanguageCode
- ActiveTargetLanguageCode
- CreatedAtUtc
- UpdatedAtUtc
```

```text
users.UserTargetLanguages
- Id
- UserId
- TargetLanguageCode
- CurrentLevel
- TargetLevel
- IsActive
- CreatedAtUtc
- UpdatedAtUtc
```

Example:

```text
NativeLanguageCode = vi
UiLanguageCode = vi
ExplanationLanguageCode = vi
ActiveTargetLanguageCode = en
```

### 9.4 Users.Contracts

```csharp
public interface IUserLanguageSettingsReader
{
    Task<UserLanguageSettingsReadModel?> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);
}
```

```csharp
public sealed record UserLanguageSettingsReadModel(
    Guid UserId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string CurrentLevel,
    string TargetLevel);
```

---

## 10. Database Strategy

### Physical database

Use:

```text
1 physical database
multiple schemas
one DbContext per module
one migration stream per module
```

### Schemas

```text
auth
users
studyplans
learningcontent
vocabulary
exercises
speaking
ai
mistakes
assessments
progress
notifications
adminreports
messaging
```

### Cross-module foreign keys

Recommended:

```text
Within the same module: use foreign keys normally.
Across modules: avoid FK and navigation property by default.
```

Instead, store IDs:

```csharp
public Guid UserId { get; private set; }
public Guid? LessonId { get; private set; }
```

Validate existence through Contract Readers if needed.

### DbContext per module

Example: Speaking module

```csharp
public sealed class SpeakingDbContext : DbContext
{
    public DbSet<SpeakingSession> SpeakingSessions => Set<SpeakingSession>();
    public DbSet<SpeakingTurn> SpeakingTurns => Set<SpeakingTurn>();
    public DbSet<SpeakingTurnResult> SpeakingTurnResults => Set<SpeakingTurnResult>();
    public DbSet<SpeakingSessionSummary> SpeakingSessionSummaries => Set<SpeakingSessionSummary>();

    public SpeakingDbContext(DbContextOptions<SpeakingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("speaking");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SpeakingDbContext).Assembly);
    }
}
```

Migration history table should be schema-specific:

```csharp
options.UseSqlServer(connectionString, sql =>
{
    sql.MigrationsHistoryTable("__EFMigrationsHistory", "speaking");
});
```

---

## 11. Cross-Module Communication

Allowed communication methods:

```text
1. Contract Reader
2. Integration Event
3. Read Model / Projection Table
4. Database View / Materialized View for admin/reporting only
```

### 11.1 Contract Reader

Use for small synchronous reads when data is needed immediately.

Examples:

```text
Speaking starts session → needs user language/level
Assessments creates level test → needs current level
LearningContent recommends lessons → needs active target language
```

Rules:

```text
- Return DTO/read model only.
- Do not return Domain Entities.
- Do not expose IQueryable.
- Interface lives in owner module Contracts.
- Implementation lives in owner module Infrastructure.
```

### 11.2 Integration Events

Use when one module's state change should notify other modules.

Examples:

```text
VocabularyReviewedIntegrationEvent
ExerciseCompletedIntegrationEvent
SpeakingSessionCompletedIntegrationEvent
MistakeCreatedIntegrationEvent
AssessmentPassedIntegrationEvent
```

Rules:

```text
- Event names should be past tense.
- Events represent business facts.
- Use Outbox for reliable publishing.
- Consumers must be idempotent with Inbox.
```

### 11.3 Read Model / Projection Table

Use for aggregate screens and user-facing query performance.

Examples:

```text
progress.UserDashboardSnapshots
learningcontent.UserLearningPathCards
vocabulary.UserVocabularyStudyCards
speaking.UserSpeakingHistoryCards
adminreports.UserOverviewCards
```

Rules:

```text
- The module that needs the read model owns it.
- Read models can duplicate data from other modules.
- Read models are updated through Integration Events.
- Read models are optimized for queries, not domain purity.
```

### 11.4 Database View / Materialized View

Use only for admin/reporting/analytics, not core business flow.

Good use cases:

```text
Admin user overview
Daily AI usage report
Common mistake statistics
Export data
Internal analytics
```

Avoid for:

```text
Core dashboard
Learning path
Main user progress
Core recommendation logic
```

---

## 12. IQueryable Rule

`IQueryable` must not cross module boundaries.

Allowed:

```text
Users module can use IQueryable internally with UsersDbContext.
Vocabulary module can use IQueryable internally with VocabularyDbContext.
Speaking module can use IQueryable internally with SpeakingDbContext.
```

Forbidden:

```text
Users.Contracts exposes IQueryable<User>
LearningContent receives IQueryable from Users
Progress directly queries UsersDbContext + SpeakingDbContext + MistakesDbContext
```

Reason:

```text
- Leaks EF Core implementation
- Breaks module boundaries
- Allows uncontrolled cross-module joins
- Makes future service extraction difficult
```

---

## 13. Event Messaging Architecture

### 13.1 Chosen approach

Use:

```text
In-process Event Dispatcher
+ Transactional Outbox per producer module
+ Inbox per consumer module
+ Retry
+ Dead-letter table
+ Worker host
```

Do not use RabbitMQ/Kafka at the beginning.

This is production-grade for a modular monolith because events are persisted in DB before dispatch.

### 13.2 Why not RabbitMQ immediately?

RabbitMQ is useful when:

```text
- Workers/services are deployed independently
- Consumers must scale independently
- External systems consume events
- Queue throughput exceeds DB-backed outbox needs
```

For this system, since the target architecture is modular monolith, DB-backed Outbox + Worker + InProcess dispatcher is the better starting point.

RabbitMQ can be added later as an adapter behind `IEventBus`.

Kafka is not recommended unless the system needs large-scale event streaming and analytics pipelines.

### 13.3 Domain Event vs Integration Event

Domain Events are internal to a module.

Example:

```text
SpeakingSessionCompletedDomainEvent
```

Integration Events are public events for other modules.

Example:

```text
SpeakingSessionCompletedIntegrationEvent
```

Flow:

```text
Aggregate changes state
→ raises DomainEvent
→ Application maps DomainEvent to IntegrationEvent
→ IntegrationEvent saved to Outbox
```

Domain entities must not know Integration Events.

### 13.4 Outbox

Producer module owns Outbox.

Example:

```text
speaking.OutboxMessages
vocabulary.OutboxMessages
exercises.OutboxMessages
assessments.OutboxMessages
studyplans.OutboxMessages
```

Recommended fields:

```text
Id
EventId
EventType
Payload
Status
RetryCount
MaxRetryCount
NextRetryAtUtc
LockedBy
LockedUntilUtc
CreatedAtUtc
ProcessedAtUtc
LastError
```

### 13.5 Inbox

Consumer module owns Inbox.

Example:

```text
progress.InboxMessages
notifications.InboxMessages
adminreports.InboxMessages
mistakes.InboxMessages
```

Recommended fields:

```text
Id
EventId
EventType
HandlerName
ProcessedAtUtc
```

Unique index:

```text
EventId + HandlerName
```

### 13.6 Dead-letter

Use global messaging schema:

```text
messaging.DeadLetterMessages
```

Recommended fields:

```text
Id
EventId
EventType
Payload
SourceModule
FailedAtUtc
RetryCount
LastError
StackTrace
Status
```

### 13.7 EventHandlerExecutions decision

Do not include `EventHandlerExecutions` in the core design initially.

Reason:

```text
Outbox + Inbox already provides reliable dispatch and idempotency.
```

Retry strategy:

```text
Retry the whole event.
Handlers that already succeeded check Inbox and return immediately.
```

`EventHandlerExecutions` is optional later if the system needs:

```text
- Retry per handler
- Handler-level operational dashboard
- Handler-level performance metrics
- Handler-level dead-lettering
```

Current core design:

```text
Per-module Outbox
Per-consumer Inbox
Global DeadLetterMessages
Structured logs
CorrelationId/EventId
```

---

## 14. Module Details

## 14.1 Auth Module

### Responsibility

Owns identity and authentication.

### Schema

```text
auth
```

### Owns tables

```text
auth.Users
auth.RefreshTokens
auth.UserCredentials
```

### Main features

```text
Register
Login
Refresh token
Logout
Password hashing
JWT generation
Role/permission basics later
```

### Must not own

```text
User learning profile
Language settings
Learning goals
Study schedule
```

Those belong to Users / StudyPlans.

---

## 14.2 Users Module

### Responsibility

Owns user profile, language settings, and target languages.

### Schema

```text
users
```

### Owns tables

```text
users.UserProfiles
users.UserLanguageSettings
users.UserTargetLanguages
users.UserPreferences
```

### Main features

```text
Display name
Native language
UI language
Explanation language
Active target language
Current level per target language
Target level per target language
Daily user preferences
```

### Exposes contracts

```text
IUserProfileReader
IUserLanguageSettingsReader
IUserTargetLanguageReader
```

### Publishes events

```text
UserRegisteredIntegrationEvent
UserProfileUpdatedIntegrationEvent
UserLanguageSettingsUpdatedIntegrationEvent
UserTargetLanguageChangedIntegrationEvent
UserLevelChangedIntegrationEvent
```

---

## 14.3 StudyPlans Module

### Responsibility

Owns learning plan, study schedule, rest days, and planned study sessions.

### Schema

```text
studyplans
```

### Owns tables

```text
studyplans.UserStudyPlans
studyplans.UserStudyWeekDays
studyplans.PlannedStudySessions
studyplans.StudyPlanTargets
```

### Main features

```text
Preferred study time
Reminder before minutes
Time zone
Daily target minutes
Weekly target minutes
Monthly target minutes
Monthly target study days
Study days in week
Rest days in week
Planned sessions
Missed sessions
```

### StudyPlans vs Progress

```text
StudyPlans = planned learning
Progress   = actual learning
```

### Publishes events

```text
StudyPlanCreatedIntegrationEvent
StudyPlanUpdatedIntegrationEvent
PlannedStudySessionCreatedIntegrationEvent
PlannedStudySessionMissedIntegrationEvent
DailyStudyTargetCompletedIntegrationEvent
```

---

## 14.4 LearningContent Module

### Responsibility

Owns structured learning content except vocabulary flashcard content.

### Schema

```text
learningcontent
```

### Owns tables

```text
learningcontent.Lessons
learningcontent.LessonTranslations
learningcontent.LessonSections
learningcontent.LessonSectionTranslations
learningcontent.ConversationScenarios
learningcontent.ConversationScenarioTranslations
learningcontent.ConversationLines
learningcontent.ConversationLineTranslations
learningcontent.SentencePatterns
learningcontent.Quizzes
learningcontent.UserLearningPathCards
```

### Main features

```text
Lessons
Lesson sections
Conversation scenarios
Conversation scripts
Sentence patterns
Mini quizzes/drills content
Learning path cards
```

### Conversation ownership

Conversation belongs to LearningContent because it is content/script.

Speaking module owns the runtime practice session.

```text
LearningContent = conversation script
Speaking        = user practice session and results
```

### Publishes events

```text
LessonPublishedIntegrationEvent
LessonCompletedIntegrationEvent
ConversationScenarioCompletedIntegrationEvent
```

### Consumes events

```text
UserProfileUpdatedIntegrationEvent
SpeakingSessionCompletedIntegrationEvent
MistakeCreatedIntegrationEvent
```

---

## 14.5 Vocabulary Module

### Responsibility

Owns vocabulary flashcards, translations, example sentences, detailed vocabulary attempts, and vocabulary mastery.

### Schema

```text
vocabulary
```

### Owns tables

```text
vocabulary.VocabularyItems
vocabulary.VocabularyTranslations
vocabulary.VocabularyExamples
vocabulary.VocabularyExampleTranslations
vocabulary.UserVocabularyProgress
vocabulary.UserVocabularyMastery
vocabulary.VocabularyReviewSessions
vocabulary.VocabularyReviewAttempts
vocabulary.VocabularyPronunciationAttempts
vocabulary.ExampleSentencePronunciationAttempts
vocabulary.UserVocabularyStudyCards
```

### Example Sentences ownership

Example Sentences belong to Vocabulary.

Reason:

```text
When the user studies a flashcard, the app should show example sentences for that vocabulary item.
```

### VocabularyItems

```text
Id
TargetLanguageCode
Word
Phonetic
Level
Topic
PartOfSpeech
CreatedAtUtc
```

### VocabularyTranslations

```text
Id
VocabularyItemId
LanguageCode
Meaning
ShortExplanation
Notes
```

### VocabularyExamples

```text
Id
VocabularyItemId
TargetLanguageCode
Sentence
DifficultyLevel
```

### VocabularyExampleTranslations

```text
Id
VocabularyExampleId
LanguageCode
Meaning
```

### Detailed attempts

`VocabularyPronunciationAttempts` should store each word pronunciation attempt:

```text
Id
UserId
VocabularyItemId
TargetLanguageCode
AudioUrl
RecognizedText
PronunciationScore
AccuracyScore
FluencyScore
CompletenessScore
Feedback
AttemptedAtUtc
```

`ExampleSentencePronunciationAttempts` should store each sentence attempt:

```text
Id
UserId
VocabularyExampleId
VocabularyItemId
TargetLanguageCode
AudioUrl
ExpectedText
RecognizedText
PronunciationScore
FluencyScore
AccuracyScore
WordLevelFeedbackJson
Feedback
AttemptedAtUtc
```

### UserVocabularyMastery

```text
UserId
VocabularyItemId
TargetLanguageCode
MeaningMasteryScore
PronunciationMasteryScore
ExampleSentenceScore
ReviewCount
CorrectReviewCount
LastReviewedAtUtc
NextReviewAtUtc
Status
```

Status:

```text
New
Learning
Reviewing
Weak
Mastered
```

### Publishes events

```text
VocabularyReviewedIntegrationEvent
VocabularyMasteredIntegrationEvent
VocabularyPronunciationPracticedIntegrationEvent
ExampleSentencePronunciationPracticedIntegrationEvent
```

---

## 14.6 Exercises Module

### Responsibility

Owns practice exercises and detailed exercise attempts/answers.

### Schema

```text
exercises
```

### Owns tables

```text
exercises.ExerciseSets
exercises.ExerciseQuestions
exercises.ExerciseOptions
exercises.UserExerciseAttempts
exercises.UserExerciseAnswers
exercises.UserExerciseResults
```

### Exercise types

Supported exercise types:

```text
MultipleChoice
FillInTheBlank
VerbConjugation
SentenceCorrection
SentenceOrdering
Translation
Matching
ListeningChoice
ConversationCompletion
ShortWriting
```

MVP exercise types:

```text
MultipleChoice
FillInTheBlank
VerbConjugation
SentenceCorrection
SentenceOrdering
```

### Static vs AI-graded exercises

Static exercises:

```text
Fixed answer
Backend can grade directly
```

AI-graded exercises:

```text
Open answer
Translation
Short writing
Conversation completion
AI grades using rubric
Backend stores final result
```

### Tables

`ExerciseSets`:

```text
Id
TargetLanguageCode
Level
Topic
Skill
ExerciseType
Title
Description
```

`ExerciseQuestions`:

```text
Id
ExerciseSetId
QuestionType
Prompt
CorrectAnswer
Explanation
Order
Difficulty
```

`ExerciseOptions`:

```text
Id
QuestionId
OptionText
IsCorrect
Order
```

`UserExerciseAttempts`:

```text
Id
UserId
ExerciseSetId
TargetLanguageCode
StartedAtUtc
CompletedAtUtc
TotalQuestions
CorrectCount
Score
Status
```

`UserExerciseAnswers`:

```text
Id
AttemptId
QuestionId
UserAnswer
IsCorrect
Score
Feedback
AnsweredAtUtc
```

### Publishes events

```text
ExerciseStartedIntegrationEvent
ExerciseQuestionAnsweredIntegrationEvent
ExerciseCompletedIntegrationEvent
```

---

## 14.7 Speaking Module

### Responsibility

Owns speaking practice runtime, speaking sessions, turns, turn results, and session summaries.

### Schema

```text
speaking
```

### Owns tables

```text
speaking.SpeakingSessions
speaking.SpeakingTurns
speaking.SpeakingTurnResults
speaking.SpeakingSessionSummaries
speaking.UserSpeakingHistoryCards
speaking.ConversationPracticeResults
```

### Language snapshot

When a speaking session starts, store:

```text
NativeLanguageCodeAtStart
TargetLanguageCodeAtStart
UiLanguageCodeAtStart
ExplanationLanguageCodeAtStart
UserLevelAtStart
```

Reason:

```text
A user may change language or level later.
Old sessions should keep the original learning context.
```

### SpeakingTurnResults

```text
Id
SpeakingTurnId
UserId
TargetLanguageCode
OriginalText
CorrectedText
NaturalVersion
GrammarScore
VocabularyScore
PronunciationScore
FluencyScore
TaskCompletionScore
OverallScore
Feedback
FeedbackLanguageCode
AudioUrl
RecognizedText
WordLevelFeedbackJson
CreatedAtUtc
```

### SpeakingSessionSummaries

```text
Id
SpeakingSessionId
UserId
TargetLanguageCode
AverageGrammarScore
AverageVocabularyScore
AveragePronunciationScore
AverageFluencyScore
OverallScore
TotalTurns
TotalMistakes
StrongPoints
WeakPoints
Recommendation
```

### Uses contracts

```text
Users.Contracts.IUserLanguageSettingsReader
AI.Contracts.IEnglishCorrectionService
LearningContent.Contracts if session is based on conversation/lesson content
```

### Must not do

```text
Must not call Gemini/Gemma clients directly
Must not reference Users.Infrastructure
Must not reference AI.Infrastructure
Must not query UsersDbContext
```

### Publishes events

```text
SpeakingSessionStartedIntegrationEvent
SpeakingTurnCorrectedIntegrationEvent
SpeakingSessionCompletedIntegrationEvent
ConversationPracticeCompletedIntegrationEvent
```

---

## 14.8 AI Module

### Responsibility

Owns AI provider integration, model routing, prompt templates, AI usage logging, quota, and cost estimation.

### Schema

```text
ai
```

### Owns tables

```text
ai.AiRequestLogs
ai.PromptTemplates
ai.PromptVersions
ai.ModelRoutingRules
ai.AiUsageCounters
ai.AiCostEstimations
```

### Main features

```text
Correct sentence
Generate daily learning plan
Generate exercises
Generate vocabulary examples
Grade open answers
Grade assessments
Generate audio sample later
Route request to Gemma/Gemini/Gemini Live/Gemini TTS
Log all AI requests
Track usage by user/model/task type
```

### Model routing rules

```text
Short sentence correction      → Gemma
Daily exercise generation      → Gemma
Vocabulary examples            → Gemma
Long writing correction         → Gemini Flash
Deep grammar explanation        → Gemini Flash
Assessment grading              → Gemini Flash or stronger model if needed
Realtime voice                  → Gemini Live
Audio sample / TTS              → Gemini TTS
```

### AI request language context

AI requests should include:

```text
NativeLanguageCode
TargetLanguageCode
ExplanationLanguageCode
UserLevel
Topic
```

### Exposes contracts

```text
IEnglishCorrectionService
IAiLessonGenerator
IAiExerciseGenerator
IAssessmentGradingService
IAudioGenerationService
```

---

## 14.9 Mistakes Module

### Responsibility

Owns user mistakes and mistake review queue.

### Schema

```text
mistakes
```

### Owns tables

```text
mistakes.Mistakes
mistakes.MistakeReviews
mistakes.MistakeCategories
mistakes.UserMistakeCards
```

### SourceType

Mistakes must track source:

```text
SourceType
SourceId
```

SourceType examples:

```text
SpeakingTurn
VocabularyPronunciationAttempt
ExampleSentencePronunciationAttempt
ExerciseAnswer
AssessmentAnswer
ConversationTurn
```

### Fields

```text
UserId
TargetLanguageCode
NativeLanguageCode
ExplanationLanguageCode
Type
Category
OriginalText
CorrectedText
Explanation
SourceType
SourceId
Status
NextReviewAtUtc
CreatedAtUtc
```

### Consumes events

```text
SpeakingTurnCorrectedIntegrationEvent
ExerciseCompletedIntegrationEvent or ExerciseQuestionAnsweredIntegrationEvent
VocabularyPronunciationPracticedIntegrationEvent
ExampleSentencePronunciationPracticedIntegrationEvent
AssessmentCompletedIntegrationEvent
```

### Publishes events

```text
MistakeCreatedIntegrationEvent
MistakeReviewedIntegrationEvent
MistakeMasteredIntegrationEvent
```

---

## 14.10 Assessments Module

### Responsibility

Owns placement tests, level-up tests, strict grading, and pass/fail decisions.

### Schema

```text
assessments
```

### Owns tables

```text
assessments.AssessmentDefinitions
assessments.AssessmentSections
assessments.AssessmentQuestions
assessments.UserAssessmentAttempts
assessments.UserAssessmentAnswers
assessments.AssessmentGradingResults
assessments.AssessmentRubrics
```

### Assessment types

```text
PlacementTest
LevelUpTest
SkillCheck
MonthlyReviewTest
```

### Level-up workflow

```text
1. User requests level-up assessment.
2. Assessments checks eligibility.
3. Assessment is generated or selected based on target language/current level.
4. User completes test.
5. AI grades open answers using rubric.
6. Backend applies strict pass/fail rules.
7. If passed, Users module updates current level through event/command flow.
8. Progress logs activity and grants EXP.
9. If failed, Mistakes/Progress/LearningContent can recommend weak areas.
```

### Important rule

AI does not directly decide level promotion.

```text
AI = grader/evaluator
Assessments = decision engine
Users = current level owner
```

### Example pass rule

```text
Total score >= 75
No skill score below 60
Speaking/Writing must meet minimum threshold if included
```

### Publishes events

```text
AssessmentStartedIntegrationEvent
AssessmentCompletedIntegrationEvent
AssessmentPassedIntegrationEvent
AssessmentFailedIntegrationEvent
LevelUpApprovedIntegrationEvent
```

---

## 14.11 Progress Module

### Responsibility

Owns actual learning progress, EXP, streak, skill progress, learning activity summary, and dashboard projections.

### Schema

```text
progress
```

### Owns tables

```text
progress.LearningActivityLogs
progress.UserExperience
progress.ExperienceTransactions
progress.UserSkillProgress
progress.UserDailyProgress
progress.UserWeeklyProgress
progress.UserMonthlyProgress
progress.UserDashboardSnapshots
progress.UserStreaks
```

### Important design rule

Progress does not store every detailed attempt.

Detailed attempts belong to owner modules:

| Detailed data                           | Owner module |
| --------------------------------------- | ------------ |
| Flashcard review attempts               | Vocabulary   |
| Word pronunciation attempts             | Vocabulary   |
| Example sentence pronunciation attempts | Vocabulary   |
| Exercise attempts/answers               | Exercises    |
| Speaking turn scores                    | Speaking     |
| Conversation practice turns             | Speaking     |
| Level test attempts                     | Assessments  |
| Mistakes                                | Mistakes     |

Progress stores summary and aggregate data.

### LearningActivityLogs

Use for activity summary:

```text
Id
UserId
TargetLanguageCode
ActivityType
ActivityId
StartedAtUtc
CompletedAtUtc
DurationSeconds
ExpEarned
Score
Result
```

ActivityType examples:

```text
LessonCompleted
VocabularyReviewed
VocabularyPronunciationPracticed
ExampleSentencePronunciationPracticed
SpeakingSessionCompleted
ExerciseCompleted
MistakeReviewed
AssessmentCompleted
DailyGoalCompleted
```

### UserSkillProgress

```text
UserId
TargetLanguageCode
Skill
Category
CurrentScore
MasteryLevel
TotalAttempts
LastPracticedAtUtc
UpdatedAtUtc
```

Skill examples:

```text
Vocabulary
Grammar
Pronunciation
Speaking
Listening
Reading
Writing
Conversation
```

Category examples:

```text
PastSimple
Articles
WordOrder
BackendVocabulary
DailyStandup
VerbConjugation
```

### EXP

`UserExperience`:

```text
UserId
TargetLanguageCode
TotalExp
CurrentAppRank
UpdatedAtUtc
```

`ExperienceTransactions`:

```text
Id
UserId
TargetLanguageCode
SourceType
SourceId
ExpAmount
Reason
CreatedAtUtc
```

### Progress scope

Progress should be scoped by:

```text
UserId + TargetLanguageCode
```

Reason:

```text
A user learning English and Japanese should have separate progress.
```

### Consumes events

```text
VocabularyReviewedIntegrationEvent
VocabularyPronunciationPracticedIntegrationEvent
ExampleSentencePronunciationPracticedIntegrationEvent
ExerciseCompletedIntegrationEvent
SpeakingSessionCompletedIntegrationEvent
MistakeReviewedIntegrationEvent
AssessmentCompletedIntegrationEvent
DailyStudyTargetCompletedIntegrationEvent
```

---

## 14.12 Notifications Module

### Responsibility

Owns notification settings, scheduled messages, delivery logs, and reminder workflows.

### Schema

```text
notifications
```

### Owns tables

```text
notifications.NotificationSettings
notifications.NotificationMessages
notifications.NotificationDeliveryLogs
notifications.NotificationTemplates
```

### Notification types

```text
StudyReminder
MissedStudyReminder
MistakeReviewReminder
VocabularyReviewReminder
WeeklyProgressSummary
MonthlyProgressSummary
AssessmentReminder
```

### Main features

```text
Study reminder before preferred study time
Missed study reminder
Mistake review reminder
Vocabulary review reminder
Weekly/monthly report notification
In-app/email/push later
```

### Uses StudyPlans

StudyPlans owns schedule.
Notifications uses events/read models to schedule messages.

Example:

```text
User studies at 20:30
ReminderBeforeMinutes = 15
Notification scheduled at 20:15
```

### Publishes events

```text
NotificationScheduledIntegrationEvent
NotificationSentIntegrationEvent
NotificationFailedIntegrationEvent
```

---

## 14.13 AdminReports Module

### Responsibility

Owns admin dashboard, reports, analytics, and operational views.

### Schema

```text
adminreports
```

### Owns tables/views

```text
adminreports.UserOverviewCards
adminreports.DailyAiUsageReports
adminreports.LearningActivityReports
adminreports.CommonMistakeStats
adminreports.AssessmentPassRateReports
adminreports.RetentionReports
adminreports.AuditLogs
```

Views/materialized views can be used here.

Good use cases:

```text
User overview
AI usage by day/model
Top mistake categories
Exercise completion rate
Assessment pass/fail rate
Study time reports
Retention reports
```

### Audit logs

AdminReports or a future Audit module should track:

```text
Admin changed user level
Admin changed content
Admin changed assessment result
Prompt template updated
AI model routing rule changed
```

---

## 15. File / Audio Storage

The system should include storage abstractions early.

Recommended interfaces:

```text
IFileStorageService
IAudioStorageService
```

Used for:

```text
Recorded speaking audio
Vocabulary pronunciation audio
Example sentence pronunciation audio
AI-generated TTS audio
Exported reports
```

Storage should not use local disk in production because API can scale horizontally.

Use object storage later:

```text
S3
Cloudflare R2
Google Cloud Storage
Azure Blob Storage
```

---

## 16. Redis Usage

Redis is useful for:

```text
Distributed cache
Rate limiting
Distributed lock for jobs if needed
Temporary AI quota counters
Notification debounce
Token blacklist if needed
```

Do not use Redis as source of truth for learning progress.

Source of truth remains database.

---

## 17. Observability

Production system should include:

```text
Structured logging
CorrelationId
EventId
RequestId
Metrics
Tracing
Health checks
Error tracking
Outbox/DeadLetter monitoring
AI usage monitoring
```

Important logs:

```text
AI request/response metadata
Outbox processing failures
Dead-letter messages
Notification delivery failures
Assessment grading anomalies
Admin actions
```

---

## 18. Core Workflows

## 18.1 Start Speaking Session

```text
1. Frontend calls POST /api/speaking/sessions.
2. Speaking module receives StartSpeakingSessionCommand.
3. Speaking calls Users.Contracts.IUserLanguageSettingsReader.
4. Users returns language/level settings.
5. Speaking creates SpeakingSession.
6. Speaking stores language/level snapshot.
7. Speaking saves session.
8. API returns sessionId.
```

## 18.2 Add Speaking Turn and Correct Sentence

```text
1. Frontend submits sentence/transcript/audio.
2. Speaking loads SpeakingSession.
3. Speaking creates SpeakingTurn with PendingCorrection.
4. Speaking calls AI.Contracts.IEnglishCorrectionService.
5. AI routes request to Gemma/Gemini.
6. AI logs request in ai.AiRequestLogs.
7. AI returns correction result.
8. Speaking stores SpeakingTurnResult.
9. Speaking saves SpeakingTurnCorrectedIntegrationEvent to outbox.
10. Worker dispatches event.
11. Mistakes creates mistakes if needed.
12. Progress updates skill progress/EXP/activity log.
13. AdminReports updates reports.
```

## 18.3 Vocabulary Flashcard Study

```text
1. User opens vocabulary study card.
2. Vocabulary returns word, meaning, phonetic, example sentences, current mastery scores.
3. User reviews meaning or pronunciation.
4. Vocabulary saves attempt.
5. Vocabulary updates UserVocabularyMastery.
6. Vocabulary saves integration event to outbox.
7. Progress updates EXP/skill progress/activity log.
8. Mistakes creates mistakes if needed.
```

## 18.4 Exercise Completion

```text
1. User starts ExerciseSet.
2. Exercises creates UserExerciseAttempt.
3. User answers questions.
4. Exercises stores UserExerciseAnswers.
5. Static answers graded by backend.
6. Open answers graded by AI if needed.
7. Exercises calculates score.
8. Exercises publishes ExerciseCompletedIntegrationEvent.
9. Mistakes creates mistakes for wrong answers.
10. Progress updates grammar/vocabulary skill progress and EXP.
```

## 18.5 Level-up Assessment

```text
1. User requests level-up assessment.
2. Assessments checks eligibility.
3. Assessment is generated or selected.
4. User completes assessment.
5. AI grades open sections using rubric.
6. Assessments applies strict pass/fail rules.
7. If passed, LevelUpApprovedIntegrationEvent is published.
8. Users updates current level.
9. Progress logs activity and grants EXP.
10. LearningContent/Exercises may recommend next content.
```

## 18.6 Dashboard Query

Bad approach:

```text
Query UsersDbContext
Query VocabularyDbContext
Query SpeakingDbContext
Query MistakesDbContext
Query ExercisesDbContext
Join in application code
```

Correct approach:

```text
1. Progress module maintains progress.UserDashboardSnapshots.
2. Events update the snapshot.
3. GET /api/progress/dashboard/today queries only ProgressDbContext.
```

---

## 19. API Endpoint Plan

## 19.1 Auth

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
```

## 19.2 Users

```text
GET /api/users/me/profile
PUT /api/users/me/profile
GET /api/users/me/language-settings
PUT /api/users/me/language-settings
GET /api/users/me/target-languages
POST /api/users/me/target-languages
PUT /api/users/me/target-languages/{id}/activate
```

## 19.3 StudyPlans

```text
GET  /api/study-plans/me
PUT  /api/study-plans/me
GET  /api/study-plans/me/schedule
PUT  /api/study-plans/me/schedule
GET  /api/study-plans/me/planned-sessions
POST /api/study-plans/me/planned-sessions/{id}/skip
```

## 19.4 LearningContent

```text
GET /api/lessons
GET /api/lessons/{id}
GET /api/learning-path
GET /api/conversations
GET /api/conversations/{id}
POST /api/lessons/{id}/complete
```

## 19.5 Vocabulary

```text
GET  /api/vocabulary/today
GET  /api/vocabulary/{id}/study-card
POST /api/vocabulary/{id}/review
POST /api/vocabulary/{id}/pronunciation-attempts
POST /api/vocabulary/examples/{exampleId}/pronunciation-attempts
POST /api/vocabulary/{id}/mark-mastered
```

## 19.6 Exercises

```text
GET  /api/exercises
GET  /api/exercises/{id}
POST /api/exercises/{id}/attempts
POST /api/exercises/attempts/{attemptId}/answers
POST /api/exercises/attempts/{attemptId}/complete
GET  /api/exercises/attempts/{attemptId}/result
```

## 19.7 Speaking

```text
POST /api/speaking/sessions
GET  /api/speaking/sessions
GET  /api/speaking/sessions/{id}
POST /api/speaking/sessions/{id}/turns
POST /api/speaking/sessions/{id}/complete
GET  /api/speaking/sessions/{id}/summary
```

## 19.8 AI

```text
POST /api/ai/correct-sentence
POST /api/ai/generate-daily-plan
POST /api/ai/generate-exercise-set
POST /api/ai/generate-vocabulary-examples
POST /api/ai/grade-answer
```

Note: Some AI endpoints may be internal only. User-facing flows should usually go through Speaking, Exercises, Vocabulary, or Assessments.

## 19.9 Mistakes

```text
GET  /api/mistakes
GET  /api/mistakes/today
GET  /api/mistakes/{id}
POST /api/mistakes/{id}/review
POST /api/mistakes/{id}/mark-mastered
```

## 19.10 Assessments

```text
GET  /api/assessments/available
POST /api/assessments/level-up
GET  /api/assessments/attempts/{id}
POST /api/assessments/attempts/{id}/answers
POST /api/assessments/attempts/{id}/submit
GET  /api/assessments/attempts/{id}/result
```

## 19.11 Progress

```text
GET /api/progress/dashboard/today
GET /api/progress/weekly
GET /api/progress/monthly
GET /api/progress/skills
GET /api/progress/experience
GET /api/progress/activities
```

## 19.12 Notifications

```text
GET  /api/notifications
POST /api/notifications/{id}/mark-read
GET  /api/notifications/settings
PUT  /api/notifications/settings
```

## 19.13 AdminReports

```text
GET /api/admin/reports/users
GET /api/admin/reports/ai-usage
GET /api/admin/reports/learning-activity
GET /api/admin/reports/mistakes
GET /api/admin/reports/assessments
GET /api/admin/audit-logs
```

---

## 20. Integration Event List

## Users

```text
UserRegisteredIntegrationEvent
UserProfileUpdatedIntegrationEvent
UserLanguageSettingsUpdatedIntegrationEvent
UserTargetLanguageChangedIntegrationEvent
UserLevelChangedIntegrationEvent
```

## StudyPlans

```text
StudyPlanCreatedIntegrationEvent
StudyPlanUpdatedIntegrationEvent
PlannedStudySessionCreatedIntegrationEvent
PlannedStudySessionMissedIntegrationEvent
DailyStudyTargetCompletedIntegrationEvent
```

## LearningContent

```text
LessonPublishedIntegrationEvent
LessonCompletedIntegrationEvent
ConversationScenarioCompletedIntegrationEvent
```

## Vocabulary

```text
VocabularyReviewedIntegrationEvent
VocabularyMasteredIntegrationEvent
VocabularyPronunciationPracticedIntegrationEvent
ExampleSentencePronunciationPracticedIntegrationEvent
```

## Exercises

```text
ExerciseStartedIntegrationEvent
ExerciseQuestionAnsweredIntegrationEvent
ExerciseCompletedIntegrationEvent
```

## Speaking

```text
SpeakingSessionStartedIntegrationEvent
SpeakingTurnCorrectedIntegrationEvent
SpeakingSessionCompletedIntegrationEvent
ConversationPracticeCompletedIntegrationEvent
```

## Mistakes

```text
MistakeCreatedIntegrationEvent
MistakeReviewedIntegrationEvent
MistakeMasteredIntegrationEvent
```

## Assessments

```text
AssessmentStartedIntegrationEvent
AssessmentCompletedIntegrationEvent
AssessmentPassedIntegrationEvent
AssessmentFailedIntegrationEvent
LevelUpApprovedIntegrationEvent
```

## Progress

```text
ExperienceGrantedIntegrationEvent
UserSkillProgressUpdatedIntegrationEvent
DailyProgressUpdatedIntegrationEvent
```

## Notifications

```text
NotificationScheduledIntegrationEvent
NotificationSentIntegrationEvent
NotificationFailedIntegrationEvent
```

---

## 21. Backend Build Roadmap

This roadmap is implementation-oriented. Architecture remains production-grade from the beginning.

## Phase 1: Foundation

```text
- Create solution and projects
- Add API host
- Add Worker host
- Add BuildingBlocks
- Add module placeholders
- Add Swagger/OpenAPI
- Add global exception middleware
- Add Result pattern
- Add architecture tests
- Add base Outbox/Inbox abstractions
```

## Phase 2: Identity and User Context

```text
- Auth module
- Users module
- Language settings
- Target languages
- JWT/Refresh token
- User profile contracts
```

## Phase 3: Vocabulary + AI Correction Core

```text
- Vocabulary module
- Vocabulary items/translations/examples
- UserVocabularyMastery
- AI module contracts
- AI request logging
- Basic sentence correction
```

## Phase 4: Speaking + Mistakes + Progress Core

```text
- Speaking sessions/turns/results
- Mistakes from speaking corrections
- Progress activity logs
- EXP transactions
- Basic skill progress
- Dashboard snapshot
```

## Phase 5: StudyPlans + Notifications

```text
- Study plans
- Study week days/rest days
- Planned study sessions
- Reminder scheduling
- Notification messages/logs
```

## Phase 6: LearningContent + Conversations

```text
- Lessons
- Lesson sections/translations
- Conversation scenarios/scripts
- Learning path cards
```

## Phase 7: Exercises

```text
- Exercise sets/questions/options
- Static grading
- AI-graded open answers
- Exercise attempts/results
- Mistakes from exercise answers
```

## Phase 8: Assessments

```text
- Placement tests
- Level-up tests
- Rubrics
- AI-assisted grading
- Backend pass/fail decision
- User level update flow
```

## Phase 9: AdminReports + Operations

```text
- Admin report read models/views
- AI usage reports
- Learning activity reports
- Mistake stats
- Assessment reports
- Audit logs
- Dead-letter monitoring
```

## Phase 10: Audio / Gemini Live Later

```text
1. Text correction
2. Audio upload
3. Transcript processing
4. Pronunciation scoring
5. TTS sample audio
6. Gemini Live realtime voice
```

---

## 22. Architecture Tests

Architecture tests are required.

Rules to enforce:

```text
- Domain must not depend on Application/Infrastructure/Presentation.
- Application must not depend on Infrastructure/Presentation.
- Modules may reference only other modules' Contracts projects.
- No module can reference another module's DbContext.
- Contracts must not expose Domain Entities.
- IQueryable must not be exposed by Contracts.
- AI clients must exist only inside AI.Infrastructure.
- Presentation must not contain business logic.
```

Recommended tools:

```text
NetArchTest
ArchUnitNET
```

---

## 23. Codex / Agents Usage Plan

Use Codex/agents as implementation helpers, not architecture decision makers.

### Required files

```text
AGENTS.md
docs/architecture.md
docs/module-boundaries.md
docs/adr/0001-modular-monolith.md
docs/adr/0002-module-owned-dbcontext.md
docs/adr/0003-cross-module-communication.md
docs/adr/0004-internal-messaging-outbox-inbox.md
```

### Recommended agent roles

```text
Architecture Guardian
Module Implementer
Test Writer
Code Reviewer
Documentation Agent
```

### Good task example

```text
Implement the Speaking module following AGENTS.md.
Use Users.Contracts.IUserLanguageSettingsReader.
Use AI.Contracts.IEnglishCorrectionService.
Do not reference Users.Infrastructure or AI.Infrastructure.
Do not expose IQueryable.
Add tests and ensure dotnet build/test pass.
```

### Bad task example

```text
Build the whole backend.
```

---

## 24. Final Recommendation

The backend should be built as:

```text
Production Modular Monolith
+ API Host horizontally scalable
+ Worker Host for events/background jobs
+ Clean Architecture per module
+ DDD tactical per module
+ DbContext per module
+ Schema per module
+ Contracts for small synchronous reads
+ Integration Events for state changes
+ Transactional Outbox per producer module
+ Inbox per consumer module
+ Retry + Dead-letter
+ Read Models for aggregate screens
+ Views only for admin/reporting
+ Multi-language support through user language settings and language-aware content tables
+ Detailed attempt/result tracking inside owner modules
+ Progress as summary/EXP/dashboard/skill aggregation
+ Architecture tests for enforcement
```

This architecture supports a serious English learning system, keeps module boundaries clean, avoids premature microservice complexity, and remains ready for horizontal scaling and future extraction if truly needed.