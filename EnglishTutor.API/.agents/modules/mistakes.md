# Mistakes module

Aggregates user mistakes across modules (Exercises, Vocabulary, Speaking) and powers the "review your mistakes" feature.

## Schema

`mistakes`

## Aggregate roots

| Aggregate   | Purpose                                                                                                                |
| ----------- | ---------------------------------------------------------------------------------------------------------------------- |
| `Mistake`   | Per-user mistake record: source module, originating attempt ID, expected vs actual, mastery state, next-review time.   |

## Contracts surface

- `IntegrationEvents/`:
  - `MistakeCreatedIntegrationEvent`
  - `MistakeReviewedIntegrationEvent`
  - `MistakeMasteredIntegrationEvent`

  Consumed by Progress (EXP / mastery counts) and StudyPlans (target completion).

## Key behaviors

- Listens to incorrect-answer events from upstream modules:
  - `Exercises.ExerciseQuestionAnsweredIntegrationEvent` (when `IsCorrect=false`)
  - `Vocabulary.VocabularyReviewedIntegrationEvent` / `ExampleFillBlankAttemptedIntegrationEvent` (when wrong)
  - `Speaking.SpeakingTurnCorrectedIntegrationEvent`
- Creates or updates a `Mistake` aggregate per `(UserId, TargetLanguageCode, SourceType, SourceItemId)`. Idempotent: re-receiving the same event won't duplicate.
- Mistake review uses lightweight spaced repetition (its own state, independent of Vocabulary SRS).
- A mistake becomes "mastered" after N consecutive correct reviews → raises `MistakeMasteredIntegrationEvent`.

## Notes for changes

- Adding a new mistake source: add the consumer in `Mistakes.Application/EventHandlers/`, never reach across into the source module's DB.
- Source linkage (back to the original attempt) is by ID only — never EF navigation across modules.
- Review state is independent from the source. Even if the source attempt is deleted, the mistake review row remains until mastered.
