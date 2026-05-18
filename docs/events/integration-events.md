# Integration Events

## Foundation Event Infrastructure

The foundation establishes the event infrastructure used by module-owned integration events:

- `IIntegrationEvent`
- `IntegrationEvent`
- `IIntegrationEventHandler<TEvent>`
- `IEventBus`
- In-process event dispatching
- Outbox, Inbox, retry, and DeadLetterMessage abstractions

## Event Contract Rules

- Event name: Must be past tense, for example `SpeakingSessionCompletedIntegrationEvent`.
- Producer module: The module that owns the state change.
- Consumer modules: Modules that react to the state change.
- Payload fields: DTO-only event payload fields. Do not expose Domain entities.
- When it is published: Application layer maps Domain Events to Integration Events when needed.
- Side effects: Consumers update their own state, read models, notifications, or projections.
- Idempotency notes: Consumers must check Inbox before processing and mark Inbox after success.
- Related workflows: Documented in `docs/workflows` when events are introduced.

## Phase 2 Event Catalog

### UserRegisteredIntegrationEvent

- Event name: `UserRegisteredIntegrationEvent`
- Producer module: Auth
- Consumer modules: Users
- Payload fields: `UserId`, `Email`, `DisplayName`, `RegisteredAtUtc`
- When it is published: After Auth registers a new user and maps `UserRegisteredDomainEvent` to an integration event.
- Side effects: Users creates the profile, default language settings, and default English target language.
- Idempotency notes: Users consumer must use Inbox keyed by event id.
- Related workflows: Auth registration and user profile bootstrap.

### UserProfileUpdatedIntegrationEvent

- Event name: `UserProfileUpdatedIntegrationEvent`
- Producer module: Users
- Consumer modules: Future read models and reporting consumers.
- Payload fields: `UserId`, `DisplayName`, `AvatarUrl`, `Bio`, `UpdatedAtUtc`
- When it is published: After profile data changes.
- Side effects: Consumers update denormalized profile read models.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: User profile management.

### UserLanguageSettingsUpdatedIntegrationEvent

- Event name: `UserLanguageSettingsUpdatedIntegrationEvent`
- Producer module: Users
- Consumer modules: Speaking, Vocabulary, Progress read models when needed.
- Payload fields: `UserId`, `NativeLanguageCode`, `UiLanguageCode`, `ExplanationLanguageCode`, `ActiveTargetLanguageCode`, `UpdatedAtUtc`
- When it is published: After user language settings change.
- Side effects: Consumers refresh language-context projections.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: User language settings management.

### UserTargetLanguageChangedIntegrationEvent

- Event name: `UserTargetLanguageChangedIntegrationEvent`
- Producer module: Users
- Consumer modules: Progress and learning-path projections.
- Payload fields: `UserId`, `TargetLanguageCode`, `CurrentLevel`, `TargetLevel`, `IsActive`, `ChangedAtUtc`
- When it is published: After a target language is added or activated/deactivated.
- Side effects: Consumers update target-language projections.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Target language management.

### UserLevelChangedIntegrationEvent

- Event name: `UserLevelChangedIntegrationEvent`
- Producer module: Users
- Consumer modules: Progress, StudyPlans, LearningContent, AI prompt-context projections.
- Payload fields: `UserId`, `TargetLanguageCode`, `PreviousLevel`, `NewLevel`, `ChangedAtUtc`
- When it is published: After a user's level changes for a target language.
- Side effects: Consumers update level-based recommendations and progress summaries.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Level progression.

## Phase 3 Event Catalog

### VocabularyReviewedIntegrationEvent

- Event name: `VocabularyReviewedIntegrationEvent`
- Producer module: Vocabulary
- Consumer modules: Progress, Mistakes when needed.
- Payload fields: `UserId`, `VocabularyItemId`, `TargetLanguageCode`, `IsCorrect`, `Score`, `MasteryStatus`, `ReviewedAtUtc`
- When it is published: After a vocabulary review attempt updates mastery.
- Side effects: Progress grants EXP and updates activity/skill/dashboard projections.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Vocabulary flashcard review.

### VocabularyMasteredIntegrationEvent

- Event name: `VocabularyMasteredIntegrationEvent`
- Producer module: Vocabulary
- Consumer modules: Progress.
- Payload fields: `UserId`, `VocabularyItemId`, `TargetLanguageCode`, `MasteredAtUtc`
- When it is published: After vocabulary mastery reaches mastered state or is manually marked mastered.
- Side effects: Progress increments mastered vocabulary counters.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Vocabulary flashcard review.

### VocabularyPronunciationPracticedIntegrationEvent

- Event name: `VocabularyPronunciationPracticedIntegrationEvent`
- Producer module: Vocabulary
- Consumer modules: Progress, Mistakes.
- Payload fields: `UserId`, `VocabularyItemId`, `TargetLanguageCode`, `PronunciationScore`, `AccuracyScore`, `FluencyScore`, `PracticedAtUtc`
- When it is published: After a vocabulary pronunciation attempt is scored.
- Side effects: Progress updates pronunciation skill; Mistakes may create pronunciation mistake cards for low scores.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Vocabulary pronunciation practice.

### ExampleSentencePronunciationPracticedIntegrationEvent

- Event name: `ExampleSentencePronunciationPracticedIntegrationEvent`
- Producer module: Vocabulary
- Consumer modules: Progress, Mistakes.
- Payload fields: `UserId`, `VocabularyExampleId`, `TargetLanguageCode`, `PronunciationScore`, `AccuracyScore`, `FluencyScore`, `PracticedAtUtc`
- When it is published: After an example-sentence pronunciation attempt is scored.
- Side effects: Progress updates pronunciation skill; Mistakes may create review cards for low scores.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Vocabulary pronunciation practice.

## Phase 4 Event Catalog

### SpeakingSessionStartedIntegrationEvent

- Event name: `SpeakingSessionStartedIntegrationEvent`
- Producer module: Speaking
- Consumer modules: Progress and reporting projections when needed.
- Payload fields: `UserId`, `SessionId`, `SessionType`, `TargetLanguageCode`, `UserLevel`
- When it is published: After a speaking session starts and captures language snapshot.
- Side effects: Consumers may create active-session projections.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Speaking session.

### SpeakingTurnCorrectedIntegrationEvent

- Event name: `SpeakingTurnCorrectedIntegrationEvent`
- Producer module: Speaking
- Consumer modules: Mistakes, Progress.
- Payload fields: `UserId`, `SessionId`, `TurnId`, `TargetLanguageCode`, `NativeLanguageCode`, `ExplanationLanguageCode`, `OriginalText`, `CorrectedText`, `GrammarScore`, `VocabularyScore`, `OverallScore`, `Mistakes`, `CorrectedAtUtc`
- When it is published: After AI correction is applied to a speaking turn.
- Side effects: Mistakes creates mistake cards; Progress updates skill progress.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Speaking session.

### SpeakingSessionCompletedIntegrationEvent

- Event name: `SpeakingSessionCompletedIntegrationEvent`
- Producer module: Speaking
- Consumer modules: Progress, AdminReports.
- Payload fields: `UserId`, `SessionId`, `TargetLanguageCode`, `TotalTurns`, `OverallScore`, `DurationSeconds`, `CompletedAtUtc`
- When it is published: After a speaking session is completed and summarized.
- Side effects: Progress grants EXP, updates streaks, skill progress, and dashboard snapshots.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Speaking session.

### ConversationPracticeCompletedIntegrationEvent

- Event name: `ConversationPracticeCompletedIntegrationEvent`
- Producer module: Speaking
- Consumer modules: Progress, AdminReports.
- Payload fields: `UserId`, `SessionId`, `ConversationScenarioId`, `TargetLanguageCode`, `OverallScore`, `TaskCompletionScore`, `DurationSeconds`, `CompletedAtUtc`
- When it is published: After a conversation-practice speaking session is completed.
- Side effects: Progress updates speaking/conversation skill summaries.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Speaking session.

### MistakeCreatedIntegrationEvent

- Event name: `MistakeCreatedIntegrationEvent`
- Producer module: Mistakes
- Consumer modules: Progress and reporting projections.
- Payload fields: `UserId`, `MistakeId`, `TargetLanguageCode`, `Type`, `SourceType`, `SourceId`, `CreatedAtUtc`
- When it is published: After Mistakes creates a mistake card from a producer event.
- Side effects: Progress/dashboard projections update mistake counts.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Mistake tracking.

### MistakeReviewedIntegrationEvent

- Event name: `MistakeReviewedIntegrationEvent`
- Producer module: Mistakes
- Consumer modules: Progress.
- Payload fields: `UserId`, `MistakeId`, `TargetLanguageCode`, `ReviewedAtUtc`
- When it is published: After a user reviews a mistake.
- Side effects: Progress grants EXP and updates activity logs.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Mistake review.

### MistakeMasteredIntegrationEvent

- Event name: `MistakeMasteredIntegrationEvent`
- Producer module: Mistakes
- Consumer modules: Progress.
- Payload fields: `UserId`, `MistakeId`, `TargetLanguageCode`, `MasteredAtUtc`
- When it is published: After a user marks a mistake as mastered.
- Side effects: Progress updates mastery and dashboard projections.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Mistake review.
