# Domain Structure Analysis: 4 Modules Using Flat Technical Folders

This document maps aggregate-centric domain structures for 4 modules using flat Entities/, Enums/, Events/ folders.

## 1. NOTIFICATIONS MODULE

**4 Independent Aggregates:**

### Aggregate 1: NotificationMessage (AR)
- **Class:** NotificationMessage : AggregateRoot<Guid>
- **Children:** NotificationDeliveryLog : Entity<Guid>
- **Enums:** NotificationType, NotificationChannel, NotificationStatus
- **EF:** Cascade delete on delivery logs

### Aggregate 2: NotificationTemplate (AR)
- Standalone message template aggregate
- **Enums:** NotificationType

### Aggregate 3: NotificationSetting (AR)
- User-scoped settings (unique index on UserId)

### Aggregate 4: UserNotificationSchedule (AR)
- Per-user-per-type schedule configuration
- **Enums:** NotificationType, NotificationFrequency, NotificationChannel

---

## 2. LEARNING CONTENT MODULE

**5 Independent Aggregates (3 with complex hierarchies):**

### Aggregate 1: Lesson (AR)
```
Lesson (AR)
├── LessonTranslation (Entity)
└── LessonSection (Entity)
    └── LessonSectionTranslation (Entity)
```
- **Events:** LessonPublishedDomainEvent (via Publish())

### Aggregate 2: ConversationScenario (AR)
```
ConversationScenario (AR)
├── ConversationScenarioTranslation (Entity)
├── ConversationLine (Entity)
│   └── ConversationLineTranslation (Entity)
```
- **Enums:** ConversationSpeaker

### Aggregate 3: SentencePattern (AR)
- Standalone grammatical pattern aggregate

### Aggregate 4: Quiz (AR)
- Standalone, references Lesson by FK

### Aggregate 5: UserLearningPathCard (AR)
- User-scoped learning path tracking
- **Enums:** ContentType, LearningPathCardStatus
- **Events:** LessonCompletedDomainEvent, ConversationScenarioCompletedDomainEvent

---

## 3. ASSESSMENTS MODULE

**2 Main Aggregates + 1 Supporting Entity:**

### Aggregate 1: AssessmentDefinition (AR)
```
AssessmentDefinition (AR)
├── AssessmentSection (Entity)
│   └── AssessmentQuestion (Entity)
└── AssessmentRubric (Entity)
```
- **Enums:** AssessmentType, AssessmentSkill

### Aggregate 2: UserAssessmentAttempt (AR)
```
UserAssessmentAttempt (AR)
└── UserAssessmentAnswer (Entity)
```
- **Enums:** AssessmentAttemptStatus
- **Events:** AssessmentStartedDomainEvent, AssessmentPassedDomainEvent, AssessmentFailedDomainEvent
- **Business Rule:** AssessmentPassRule (checks totalScore >= passingScore AND all skills >= minSkillScore)

### Supporting Entity: AssessmentGradingResult
- 1:1 with UserAssessmentAttempt (FK: AttemptId)
- Separate audit/history record

---

## 4. EXERCISES MODULE

**Already partially aggregate-centric** (ExerciseSet/, UserExerciseAttempt/ folders exist)

### Aggregate 1: ExerciseSet (AR)
```
ExerciseSet (AR)
└── ExerciseQuestion (Entity)
    └── ExerciseOption (Entity)
```
- **Enums:** ExerciseType, QuestionDifficulty
- Location: ExerciseSet/ExerciseSet.cs

### Aggregate 2: UserExerciseAttempt (AR)
```
UserExerciseAttempt (AR)
└── UserExerciseAnswer (Entity)

Related (1:1):
UserExerciseResult (Entity)
```
- **Enums:** ExerciseAttemptStatus
- **Events:** ExerciseStartedDomainEvent, ExerciseQuestionAnsweredDomainEvent, ExerciseCompletedDomainEvent
- Location: UserExerciseAttempt/UserExerciseAttempt.cs

---

## Key Patterns

### 1. Translation Pattern (Lesson, ConversationScenario)
Multi-language support via child Translation entities with their own nested translations.

### 2. User-Scoped Aggregates
- UserNotificationSchedule (per user, per type)
- UserLearningPathCard (per user, per content)
- UserAssessmentAttempt (per user attempt)
- UserExerciseAttempt (per user attempt)

### 3. 1:1 Related Entities (Not Fully Owned)
- AssessmentGradingResult (denormalized state)
- UserExerciseResult (denormalized state)

### 4. Enum Ownership
- **Notifications:** 4 enums (Channel, Status, Type, Frequency)
- **LearningContent:** 3 enums (ContentType, Speaker, PathCardStatus)
- **Assessments:** 3 enums (Type, Skill, AttemptStatus)
- **Exercises:** 3 enums (Type, AttemptStatus, QuestionDifficulty)

### 5. Domain Events
- **Notifications:** None (integration events handled at application layer)
- **LearningContent:** 3 events from Lesson + UserLearningPathCard
- **Assessments:** 3 events from UserAssessmentAttempt
- **Exercises:** 3 events from UserExerciseAttempt

### 6. Business Rules
- **Assessments:** AssessmentPassRule (checks scores against thresholds)
- Others: Logic embedded in aggregate methods

---

## EF Configuration Insights

1. **Navigation Property Access:** `SetPropertyAccessMode(PropertyAccessMode.Field)` for backing fields
2. **Ownership:** `HasMany().WithMany()` with FK relationships
3. **Cascade Delete:** Configured on child entities (DeliveryLogs cascade from NotificationMessage)
4. **Unique Indices:** Enforce aggregate boundaries (e.g., NotificationSetting.UserId)
5. **Separate Storage:** Some related entities stored as separate tables with FK (AssessmentGradingResult, UserExerciseResult)

