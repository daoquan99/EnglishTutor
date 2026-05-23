# Progress module

EXP, streaks, skill progress, and dashboard read models. Stores **summaries**, not every detailed attempt — those live in the owner modules (Vocabulary, Exercises, Speaking, Assessments, Mistakes).

## Schema

`progress`

## Aggregate roots

| Aggregate         | Purpose                                                                                                                       |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| `UserExperience`  | Per-user, per-target-language EXP balance, current level (computed via formula), streak day count, last-active business date. |

Dashboard read models / daily-weekly-monthly snapshots live in this schema as tables updated by event handlers — not separate aggregates with rich behavior.

## Contracts surface

No integration events produced. Progress is downstream — it consumes events from many modules.

## Key behaviors

Listens to (and is the canonical EXP / streak source for):

- Vocabulary: review, mastery, pronunciation, fill-blank events.
- Exercises: completion event.
- Speaking: session-completed event.
- Mistakes: mastered event.
- Assessments: passed event.
- LearningContent: lesson-completed, conversation-scenario-completed events.
- StudyPlans: daily-target-completed event.

For each event:
- Compute EXP delta from a config / table.
- Update streak: requires at least one qualifying activity per user calendar day, where "day" is computed using the user's timezone (from Users via Contract Reader).
- Update daily/weekly/monthly snapshot rows.

Idempotency: keyed on `(EventId, ConsumerName)` via Inbox.

## Notes for changes

- Don't try to recompute EXP retroactively across all history — apply deltas via events. Backfill via a dedicated Worker job if needed.
- Streak boundary is the user's calendar day, not server UTC day.
- Dashboard widgets that aggregate cross-module data: build them as a read model here, updated incrementally via events. Don't query other modules' tables live.
- Detailed attempts belong in the source module. Do not pull every speaking turn or every vocab review into the progress schema.
