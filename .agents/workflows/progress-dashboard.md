# Progress Dashboard Workflow

## Overview
Dashboard query returns pre-computed snapshot, updated incrementally via events.

## Correct Approach (Read Model)
```
GET /api/progress/dashboard/today → queries only ProgressDbContext
→ returns UserDashboardSnapshot (pre-computed)
```

## BAD Approach (Forbidden)
```
Query UsersDbContext + VocabularyDbContext + SpeakingDbContext + MistakesDbContext
→ Join in application code ← VIOLATES MODULE BOUNDARIES
```

## How Dashboard is Updated
```
Events from all modules → Progress event handlers → update UserDashboardSnapshot incrementally

VocabularyReviewedEvent → update VocabularyMastered count
SpeakingSessionCompletedEvent → update TotalSpeakingSessions, update skill scores
ExerciseCompletedEvent → update TotalExercisesCompleted
UserLevelChangedEvent → update CurrentLevel
MistakeCreatedEvent → update TotalMistakes
```

## Dashboard Fields
TotalExp, CurrentLevel, StreakDays, VocabularyMastered, TotalSpeakingSessions, TotalExercisesCompleted, TotalMistakes, WeakSkills, StrongSkills, LastUpdatedAtUtc

## Modules Involved
Progress (owner), all learning modules (event producers)
