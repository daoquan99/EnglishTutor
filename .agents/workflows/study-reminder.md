# Study Reminder Workflow

## Overview
System schedules study reminders before preferred study time, detects missed sessions, and sends notifications.

## Reminder Flow
```
1. User creates StudyPlan with PreferredStudyTime + ReminderBeforeMinutes
2. Worker → StudyReminderSchedulingJob (every 5 min)
3. Job → finds users where reminder time falls within next 5 min (timezone-aware)
4. Job → checks user's NotificationSetting.StudyReminderEnabled
5. Job → checks today is a study day for this user
6. Job → creates NotificationMessage (type=StudyReminder)
```

## Missed Session Detection
```
1. Worker → MissedSessionDetectionJob (every 15 min)
2. Job → queries PlannedStudySessions where ScheduledDateUtc < UtcNow - 2 hours AND Status = Planned
3. Job → marks sessions as Missed
4. Job → saves PlannedStudySessionMissedIntegrationEvent to studyplans Outbox
5. Worker → dispatches to Notifications.PlannedStudySessionMissedEventHandler
6. Notifications → checks MissedStudyReminderEnabled
7. Notifications → creates missed study reminder notification
```

## Modules Involved
StudyPlans, Notifications (consumer), Worker (job host)

## Events
- `PlannedStudySessionMissedIntegrationEvent` → Notifications
- `DailyStudyTargetCompletedIntegrationEvent` → Notifications, Progress
