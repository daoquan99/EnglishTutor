# Progress API

All endpoints require authentication and read Progress-owned summaries only.

## Endpoints

| Endpoint | Purpose |
| --- | --- |
| `GET /api/progress/dashboard/today?targetLanguageCode=en` | Current dashboard snapshot. |
| `GET /api/progress/weekly?targetLanguageCode=en&year=2026&weekNumber=21` | Weekly EXP/activity summary. |
| `GET /api/progress/monthly?targetLanguageCode=en&year=2026&month=5` | Monthly EXP/activity summary. |
| `GET /api/progress/skills?targetLanguageCode=en` | Skill progress breakdown. |
| `GET /api/progress/experience?targetLanguageCode=en` | Total EXP and rank. |
| `GET /api/progress/activities?targetLanguageCode=en` | Recent activity logs. |

## Request Body

These endpoints are query-only and do not use request bodies.

## Response Bodies

Dashboard:

```json
{ "userId": "00000000-0000-0000-0000-000000000000", "targetLanguageCode": "en", "date": "2026-05-18", "totalExp": 100, "currentLevel": "A1", "streakDays": 3, "vocabularyMastered": 10, "totalSpeakingSessions": 2, "totalExercisesCompleted": 0, "totalMistakes": 5, "weakSkills": "Grammar", "strongSkills": "Vocabulary" }
```

Weekly/monthly:

```json
{ "userId": "00000000-0000-0000-0000-000000000000", "targetLanguageCode": "en", "year": 2026, "weekNumber": 21, "expEarned": 120, "activityCount": 8 }
```

Skills:

```json
[{ "skill": "Speaking", "score": 82, "activityCount": 4, "updatedAtUtc": "2026-05-18T00:00:00Z" }]
```

Experience:

```json
{ "userId": "00000000-0000-0000-0000-000000000000", "targetLanguageCode": "en", "totalExp": 120, "currentAppRank": "Bronze" }
```

## Validation Rules

- `targetLanguageCode` is required.
- Week number must be 1-53 when supplied.
- Month must be 1-12 when supplied.

## Error Codes

- `Error.Validation`: invalid query input.
- `Error.Unauthorized`: missing/invalid JWT.

## Application Flow

- Progress consumes Vocabulary, Speaking, and Mistakes events through Inbox.
- Handlers create activity logs, grant EXP, update skill progress, streaks, daily/weekly/monthly summaries, and dashboard snapshots.
- API queries never join directly to producer module DbContexts.

## Related Modules

- Vocabulary, Speaking, and Mistakes publish events consumed by Progress.
- AdminReports may later read progress projections through its own contracts/projections.

## Integration Events Produced

- None.

## Integration Events Consumed

- `VocabularyReviewedIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExampleSentencePronunciationPracticedIntegrationEvent`
- `SpeakingTurnCorrectedIntegrationEvent`
- `SpeakingSessionCompletedIntegrationEvent`
- `MistakeReviewedIntegrationEvent`

## Read Models/Projections Updated

- `LearningActivityLog`
- `UserExperience`
- `UserSkillProgress`
- `UserDailyProgress`
- `UserWeeklyProgress`
- `UserMonthlyProgress`
- `UserDashboardSnapshot`
- `UserStreak`
