# StudyPlans Module

## Responsibility
Owns learning plan, study schedule, rest days, and planned study sessions.

## Schema
`studyplans`

## Tables
- `studyplans.UserStudyPlans` — PreferredStudyTimeUtc, ReminderBeforeMinutes, TimeZoneId, DailyTargetMinutes
- `studyplans.UserStudyWeekDays` — DayOfWeek, IsStudyDay
- `studyplans.PlannedStudySessions` — ScheduledDateUtc, Status (Planned/Completed/Missed/Skipped)
- `studyplans.StudyPlanTargets` — Period (Daily/Weekly/Monthly), TargetMinutes

## StudyPlans vs Progress
- StudyPlans = planned learning
- Progress = actual learning

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/study-plans/me` | Yes |
| PUT | `/api/study-plans/me` | Yes |
| GET | `/api/study-plans/me/schedule` | Yes |
| PUT | `/api/study-plans/me/schedule` | Yes |
| GET | `/api/study-plans/me/planned-sessions` | Yes |
| POST | `/api/study-plans/me/planned-sessions/{id}/skip` | Yes |

## Events Produced
- `StudyPlanCreatedIntegrationEvent`
- `StudyPlanUpdatedIntegrationEvent`
- `PlannedStudySessionCreatedIntegrationEvent`
- `PlannedStudySessionMissedIntegrationEvent`
- `DailyStudyTargetCompletedIntegrationEvent`
