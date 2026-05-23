# Speaking module

Speech-based learning sessions: multi-turn AI conversations and per-turn pronunciation scoring.

## Schema

`speaking`

## Aggregate roots

| Aggregate          | Purpose                                                                                                  |
| ------------------ | -------------------------------------------------------------------------------------------------------- |
| `SpeakingSession`  | One conversation/practice session: scenario, turns, transcripts, scores, AI feedback, session summary.   |

`SpeakingSession` is a single rich aggregate that owns child entities for turns, turn results, and the session summary. Don't split these into separate aggregates — the session lifecycle binds them.

## Contracts surface

- `DTOs/` — shared DTOs (e.g., session-summary view).
- `IntegrationEvents/`:
  - `SpeakingSessionStartedIntegrationEvent`
  - `SpeakingTurnCorrectedIntegrationEvent` — per AI-corrected turn. Consumed by Mistakes.
  - `SpeakingSessionCompletedIntegrationEvent` — consumed by Progress, StudyPlans, Notifications (session summary).
  - `ConversationPracticeCompletedIntegrationEvent` — when based on a `ConversationScenario` from LearningContent.

## Key behaviors

- Language pair is **snapshotted on the session at start** (`NativeLanguageCode`, `TargetLanguageCode`, `ExplanationLanguageCode`, level). Subsequent profile changes do not retroactively change the session.
- AI calls (transcription, grading, turn correction, session summary) go through `AI.Contracts`.
- Audio storage uses `IStorageService` from BuildingBlocks (S3-compatible or local).
- Per-turn scoring contains: pronunciation score, fluency cues, grammar corrections, AI explanation.

## Notes for changes

- Session lifecycle: `Pending → Active → Completed | Abandoned`. Don't introduce intermediate statuses without updating consumers.
- Audio URLs in events are short-lived signed URLs — consumers should resolve via the storage service when needed, not cache the URL.
- Speaking conversation scenarios come from `LearningContent.Contracts` (Contract Reader). Speaking doesn't own scenario content.
