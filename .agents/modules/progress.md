# Progress Module

## Responsibility
Owns actual learning progress, EXP, streak, skill progress, dashboard projections.

## Schema
`progress`

## Tables
- `progress.LearningActivityLogs` — UserId, ActivityType, ActivityId, DurationSeconds, ExpEarned, Score
- `progress.UserExperience` — TotalExp, CurrentAppRank per UserId+TargetLanguageCode
- `progress.ExperienceTransactions` — SourceType, SourceId, ExpAmount, Reason
- `progress.UserSkillProgress` — Skill, Category, CurrentScore, MasteryLevel, TotalAttempts
- `progress.UserDailyProgress` — Daily summaries
- `progress.UserWeeklyProgress` — Weekly summaries
- `progress.UserMonthlyProgress` — Monthly summaries
- `progress.UserDashboardSnapshots` — Pre-computed dashboard data
- `progress.UserStreaks` — CurrentStreakDays, LongestStreakDays, LastActivityDateUtc

## Key Rule
Progress stores SUMMARY data only. Detailed attempts belong to owner modules (Vocabulary, Exercises, Speaking, Assessments, Mistakes).

## Progress Scope
All progress scoped by `UserId + TargetLanguageCode`.

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/progress/dashboard/today` | Yes |
| GET | `/api/progress/weekly` | Yes |
| GET | `/api/progress/monthly` | Yes |
| GET | `/api/progress/skills` | Yes |
| GET | `/api/progress/experience` | Yes |
| GET | `/api/progress/activities` | Yes |

## Events Consumed
- `VocabularyReviewedIntegrationEvent` → log activity, grant EXP, update skills
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExerciseCompletedIntegrationEvent`
- `SpeakingSessionCompletedIntegrationEvent`
- `MistakeReviewedIntegrationEvent`
- `AssessmentCompletedIntegrationEvent`
- `DailyStudyTargetCompletedIntegrationEvent`
- `UserLevelChangedIntegrationEvent` → grant bonus EXP

## Events Produced
- `ExperienceGrantedIntegrationEvent`
- `UserSkillProgressUpdatedIntegrationEvent`
- `DailyProgressUpdatedIntegrationEvent`
