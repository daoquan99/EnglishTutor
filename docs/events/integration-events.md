# Integration Events

## Assessments

### AssessmentStartedIntegrationEvent

- Event name: `AssessmentStartedIntegrationEvent`
- Producer module: Assessments
- Consumer modules: AdminReports, if projection handlers are enabled
- Payload fields: `UserId`, `AssessmentAttemptId`, `AssessmentDefinitionId`, `TargetLanguageCode`, `CurrentLevel`, `StartedAtUtc`
- When it is published: A learner starts an assessment attempt.
- Side effects: Operational projections may record the attempt start.
- Idempotency notes: Consumers must use Inbox.
- Related workflows: Level-Up Assessment

### AssessmentCompletedIntegrationEvent

- Event name: `AssessmentCompletedIntegrationEvent`
- Producer module: Assessments
- Consumer modules: AdminReports, Progress if assessment activity projection is enabled
- Payload fields: `UserId`, `AssessmentAttemptId`, `AssessmentDefinitionId`, `TargetLanguageCode`, `AssessmentType`, `Score`, `IsPassed`, `CompletedAtUtc`
- When it is published: Assessment grading completes.
- Side effects: Reporting projections can update pass/fail and activity reports.
- Idempotency notes: Consumers must use Inbox.
- Related workflows: Level-Up Assessment

### AssessmentPassedIntegrationEvent

- Event name: `AssessmentPassedIntegrationEvent`
- Producer module: Assessments
- Consumer modules: AdminReports
- Payload fields: `UserId`, `AssessmentAttemptId`, `AssessmentDefinitionId`, `TargetLanguageCode`, `Score`, `PassedAtUtc`
- When it is published: Domain pass rule marks an attempt as passed.
- Side effects: Reporting projections can update pass counts.
- Idempotency notes: Consumers must use Inbox.
- Related workflows: Level-Up Assessment

### AssessmentFailedIntegrationEvent

- Event name: `AssessmentFailedIntegrationEvent`
- Producer module: Assessments
- Consumer modules: AdminReports
- Payload fields: `UserId`, `AssessmentAttemptId`, `AssessmentDefinitionId`, `TargetLanguageCode`, `Score`, `WeakSkills`, `FailedAtUtc`
- When it is published: Domain pass rule marks an attempt as failed.
- Side effects: Reporting projections can update weak skill and fail counts.
- Idempotency notes: Consumers must use Inbox.
- Related workflows: Level-Up Assessment

### LevelUpApprovedIntegrationEvent

- Event name: `LevelUpApprovedIntegrationEvent`
- Producer module: Assessments
- Consumer modules: Users, AdminReports
- Payload fields: `UserId`, `TargetLanguageCode`, `PreviousLevel`, `NewLevel`, `AssessmentScore`, `AssessmentAttemptId`, `ApprovedAtUtc`
- When it is published: A level-up assessment passes and a next level exists.
- Side effects: Users updates the target language level and publishes `UserLevelChangedIntegrationEvent`.
- Idempotency notes: Users checks Inbox before updating level.
- Related workflows: Level-Up Assessment

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

## AdminReports

### ProgressSummaryReadyIntegrationEvent

- Event name: `ProgressSummaryReadyIntegrationEvent`
- Producer module: AdminReports
- Consumer modules: Notifications
- Payload fields: `UserId`, `Period`, `StartDate`, `EndDate`, `ActivityCount`, `ExpEarned`, `GeneratedAtUtc`
- When it is published: A weekly or monthly Worker report job persists AdminReports aggregate report rows for a user activity period.
- Side effects: Notifications creates a weekly or monthly progress summary notification when the user's summary setting is enabled.
- Idempotency notes: AdminReports saves the event to Outbox with report data; Notifications checks Inbox and also avoids duplicate user/type/scheduled-date notifications.
- Related workflows: Admin Reports And Operations.

## Phase 2 Event Catalog

### UserRegisteredIntegrationEvent

- Event name: `UserRegisteredIntegrationEvent`
- Producer module: Auth
- Consumer modules: Users
- Payload fields: `UserId`, `Email`, `DisplayName`, `RegisteredAtUtc`
- When it is published: After Auth registers a new user and maps `UserRegisteredDomainEvent` to an integration event.
- Side effects: Users creates the profile, default language settings, and default English target language. AdminReports creates a user overview projection card.
- Idempotency notes: Users and AdminReports consumers must use Inbox keyed by event id.
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
- When it is published: After a target language is added, activated, or deactivated. Added events carry the actual `IsActive` state.
- Side effects: Consumers update target-language projections.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Target language management.

### UserLevelChangedIntegrationEvent

- Event name: `UserLevelChangedIntegrationEvent`
- Producer module: Users
- Consumer modules: Progress, StudyPlans, LearningContent, AdminReports, AI prompt-context projections.
- Payload fields: `UserId`, `TargetLanguageCode`, `PreviousLevel`, `NewLevel`, `ChangedAtUtc`
- When it is published: After a user's level changes for a target language.
- Side effects: Consumers update level-based recommendations, progress summaries, and admin user overview projections.
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
- Consumer modules: Progress, AdminReports.
- Payload fields: `UserId`, `VocabularyItemId`, `TargetLanguageCode`, `MasteredAtUtc`
- When it is published: After vocabulary mastery reaches mastered state or is manually marked mastered.
- Side effects: Progress increments mastered vocabulary counters. AdminReports increments user overview vocabulary mastery counters.
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
- Side effects: Progress grants EXP, updates streaks, skill progress, and dashboard snapshots. AdminReports increments the user's completed speaking-session count.
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
# Phase 5/6 Events

## StudyPlans

- `StudyPlanCreatedIntegrationEvent`: produced when a user study plan is created.
- `StudyPlanUpdatedIntegrationEvent`: produced when settings or schedule changes.
- `PlannedStudySessionCreatedIntegrationEvent`: produced when planned sessions are generated.
- `PlannedStudySessionMissedIntegrationEvent`: produced when Worker marks a planned session missed.
- `DailyStudyTargetCompletedIntegrationEvent`: consumed by Notifications and Progress when daily goal completion is emitted.

## LearningContent

- `LessonPublishedIntegrationEvent`: produced when a lesson is published.
- `LessonCompletedIntegrationEvent`: produced when a user completes a lesson path card; consumed by Progress.
- `ConversationScenarioCompletedIntegrationEvent`: produced when a conversation scenario is completed; consumed by Progress.

## Notifications

- Consumes `UserRegisteredIntegrationEvent` to create default notification settings.
- Consumes `PlannedStudySessionMissedIntegrationEvent` to create missed study reminders.
- Consumes `DailyStudyTargetCompletedIntegrationEvent` to create daily target completion notifications.
- Consumes `ProgressSummaryReadyIntegrationEvent` to create weekly and monthly progress summary notifications.

# Phase 7 Events

## Exercises

### ExerciseStartedIntegrationEvent

- Event name: `ExerciseStartedIntegrationEvent`
- Producer module: Exercises
- Consumer modules: Reporting/projections when needed.
- Payload fields: `UserId`, `AttemptId`, `ExerciseSetId`, `TargetLanguageCode`, `StartedAtUtc`
- When it is published: After an exercise attempt starts.
- Side effects: Consumers may create active-attempt projections.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Exercise completion.

### ExerciseQuestionAnsweredIntegrationEvent

- Event name: `ExerciseQuestionAnsweredIntegrationEvent`
- Producer module: Exercises
- Consumer modules: Reporting/projections when needed.
- Payload fields: `UserId`, `AttemptId`, `ExerciseSetId`, `QuestionId`, `TargetLanguageCode`, `IsCorrect`, `Score`, `AnsweredAtUtc`
- When it is published: After an answer is recorded.
- Side effects: Consumers may update answer analytics.
- Idempotency notes: Consumers must ignore duplicate event ids through Inbox.
- Related workflows: Exercise completion.

### ExerciseCompletedIntegrationEvent

- Event name: `ExerciseCompletedIntegrationEvent`
- Producer module: Exercises
- Consumer modules: Mistakes, Progress, AdminReports.
- Payload fields: `UserId`, `ExerciseSetId`, `AttemptId`, `TargetLanguageCode`, `ExerciseType`, `Score`, `CorrectCount`, `TotalQuestions`, `TimeTakenSeconds`, `WrongAnswers`, `CompletedAtUtc`
- When it is published: After all questions are answered and the attempt is completed.
- Side effects: Mistakes creates mistake cards for wrong answers; Progress grants EXP and updates activity, skills, period summaries, dashboard data, and streak; AdminReports increments exercise completion counters.
- Idempotency notes: Mistakes, Progress, and AdminReports check Inbox by event id and handler name before processing.
- Related workflows: Exercise completion.
