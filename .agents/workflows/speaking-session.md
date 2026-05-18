# Speaking Session Workflow

## Overview
User practices speaking by submitting text/audio turns. Each turn is corrected by AI. Session ends with a summary.

## Main Flow
```
1. User → POST /api/speaking/sessions (start session)
2. Speaking → calls Users.Contracts.IUserLanguageSettingsReader (get language/level)
3. Speaking → creates SpeakingSession with language snapshot
4. User → POST /api/speaking/sessions/{id}/turns (submit text/audio)
5. Speaking → creates SpeakingTurn
6. Speaking → calls AI.Contracts.IEnglishCorrectionService (get correction)
7. Speaking → stores SpeakingTurnResult (scores + feedback)
8. Speaking → saves SpeakingTurnCorrectedIntegrationEvent to Outbox
9. Worker → dispatches event to:
   - Mistakes → creates mistakes from corrections
   - Progress → updates skill progress
10. User → POST /api/speaking/sessions/{id}/complete
11. Speaking → calculates averages → creates SpeakingSessionSummary
12. Speaking → saves SpeakingSessionCompletedIntegrationEvent to Outbox
13. Worker → dispatches to Progress (EXP + activity log + dashboard)
```

## Modules Involved
Speaking, Users (contract), AI (contract), Mistakes (consumer), Progress (consumer)

## Events Published
- `SpeakingTurnCorrectedIntegrationEvent` → Mistakes, Progress
- `SpeakingSessionCompletedIntegrationEvent` → Progress, LearningContent, AdminReports

## Failure Behavior
- AI correction fails → SpeakingTurn Status = Failed, user can retry
- Event delivery fails → Outbox retries with exponential backoff → Dead-letter after 5 retries
