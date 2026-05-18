# Speaking Session

## Overview

Speaking owns session runtime state, turns, correction results, and session summaries. It uses Users.Contracts for language context and AI.Contracts for corrections.

## Main Flow

1. Speaking reads the user's language settings from Users.Contracts.
2. Speaking starts a session and stores an immutable language snapshot.
3. User submits a turn.
4. Speaking calls AI.Contracts for correction.
5. Speaking stores the turn result and publishes correction events through Outbox.
6. User completes the session.
7. Speaking calculates summary data and publishes completion events.

## Detailed Steps

- `SpeakingSession.Start` captures native, target, UI, explanation language, and level at session start.
- `SpeakingSession.AddTurn` only works while the session is active.
- `SpeakingSession.ApplyCorrection` marks the turn corrected, stores correction/mistake payload details, and raises a correction domain event.
- `SpeakingSession.Complete` enforces status transition and raises completion data; the summary counts mistakes from stored turn correction payloads.
- Mistakes consumes turn-corrected events to create mistake cards.
- Progress consumes turn-corrected events to update speaking, grammar, and vocabulary skill progress.
- Progress consumes completed-session events to grant EXP, update streak, and refresh dashboard snapshots.

## Modules Involved

- Speaking
- Users
- AI
- Mistakes
- Progress
- Worker

## Contracts Used

- `Users.Contracts.Readers.IUserLanguageSettingsReader`
- `AI.Contracts.Services.IEnglishCorrectionService`
- `SpeakingTurnCorrectedIntegrationEvent`
- `SpeakingSessionCompletedIntegrationEvent`

## Events Published/Consumed

- Published by Speaking: `SpeakingSessionStartedIntegrationEvent`, `SpeakingTurnCorrectedIntegrationEvent`, `SpeakingSessionCompletedIntegrationEvent`, `ConversationPracticeCompletedIntegrationEvent`.
- Consumed by Mistakes: `SpeakingTurnCorrectedIntegrationEvent`.
- Consumed by Progress: speaking correction/completion events.

## Read Models/Projections Updated

- Mistake cards
- Progress activity logs
- Progress skill summaries
- Progress dashboard snapshots
- AdminReports projections when needed

## Failure/Retry Behavior

- Speaking saves events through its own Outbox.
- Worker retries failed dispatch.
- Mistakes and Progress must use Inbox to skip duplicate events.
