# Notifications Module

## Responsibility
Owns notification settings, scheduled messages, delivery logs, reminder workflows.

## Schema
`notifications`

## Tables
- `notifications.NotificationSettings` — Per-type enable/disable flags, PreferredChannel
- `notifications.NotificationMessages` — Type, Title, Body, Status (Pending/Sent/Failed/Read), Channel
- `notifications.NotificationDeliveryLogs` — Delivery tracking
- `notifications.NotificationTemplates` — Templates per type per language with placeholders

## Notification Types
StudyReminder, MissedStudyReminder, MistakeReviewReminder, VocabularyReviewReminder, WeeklyProgressSummary, MonthlyProgressSummary, AssessmentReminder, LevelUpCongratulations

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/notifications` | Yes |
| POST | `/api/notifications/{id}/mark-read` | Yes |
| GET | `/api/notifications/settings` | Yes |
| PUT | `/api/notifications/settings` | Yes |

## Events Consumed
- `PlannedStudySessionMissedIntegrationEvent` → missed study reminder
- `DailyStudyTargetCompletedIntegrationEvent` → congratulations
- `LevelUpApprovedIntegrationEvent` → level-up congratulations
- `UserRegisteredIntegrationEvent` → create default notification settings
