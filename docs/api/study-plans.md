# StudyPlans API

## Overview

StudyPlans owns planned learning schedules and planned study session lifecycle.

## Endpoints

### `POST /api/study-plans/me`
- Purpose: Create the current user's active study plan for a target language.
- Auth requirement: Authenticated user.
- Request body: `targetLanguageCode`, `preferredStudyTime`, `reminderBeforeMinutes`, `timeZoneId`, `dailyTargetMinutes`, `weeklyTargetMinutes`, `monthlyTargetMinutes`, `monthlyTargetStudyDays`, optional `studyDays`.
- Response body: `StudyPlanResponse` with plan settings and week schedule.
- Validation rules: language code 2-3 chars, reminder 5-60 minutes, targets 5-240 minutes, monthly study days 1-31, valid timezone. `preferredStudyTime` is a local wall-clock time interpreted with `timeZoneId`; generated session timestamps are stored as UTC.
- Error codes: `Error.Conflict`, `Error.Validation`.
- Application flow: validate uniqueness and timezone, create plan, generate next seven planned sessions, save outbox events.
- Related modules: Notifications, Progress.
- Integration events produced: `StudyPlanCreatedIntegrationEvent`, `PlannedStudySessionCreatedIntegrationEvent`.

### `GET /api/study-plans/me`
- Purpose: Get active plan. Query: `targetLanguageCode` defaults to `en`.
- Auth requirement: Authenticated user.
- Response body: `StudyPlanResponse`.
- Error codes: `Error.NotFound`.
- Integration events produced: none.

### `PUT /api/study-plans/me`
- Purpose: Update plan settings. Query: `targetLanguageCode`.
- Auth requirement: Authenticated user.
- Request body: nullable plan setting fields, including `preferredStudyTime` as local wall-clock time for the stored plan timezone.
- Response body: `StudyPlanResponse`.
- Validation rules: provided numeric fields must stay inside create limits.
- Integration events produced: `StudyPlanUpdatedIntegrationEvent`.

### `GET /api/study-plans/me/schedule`
- Purpose: Get week-day study/rest settings.
- Auth requirement: Authenticated user.
- Response body: list of `{ dayOfWeek, isStudyDay }`.
- Integration events produced: none.

### `PUT /api/study-plans/me/schedule`
- Purpose: Update study/rest days and regenerate future planned sessions.
- Auth requirement: Authenticated user.
- Request body: `days[]` with `dayOfWeek`, `isStudyDay`.
- Response body: updated week schedule.
- Validation rules: at least one study day.
- Integration events produced: `StudyPlanUpdatedIntegrationEvent`, `PlannedStudySessionCreatedIntegrationEvent`.

### `GET /api/study-plans/me/planned-sessions`
- Purpose: List planned sessions. Query: `fromDateUtc`, `toDateUtc`, `status`.
- Auth requirement: Authenticated user.
- Response body: list of planned session DTOs.
- Validation rules: `fromDateUtc <= toDateUtc`.
- Integration events produced: none.

### `POST /api/study-plans/me/planned-sessions/{id}/skip`
- Purpose: Skip a planned session owned by current user.
- Auth requirement: Authenticated user.
- Response body: updated planned session DTO.
- Error codes: `Error.NotFound`, `Error.Forbidden`.
- Integration events produced: none.
