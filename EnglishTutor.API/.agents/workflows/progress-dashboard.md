# Workflow — Progress dashboard

The main learner dashboard: streak, today's progress, weekly graph, recent mistakes, due cards. Read-only from the client's view; built incrementally from integration events.

## Modules involved

Progress (owner of dashboard read models), Users, Vocabulary, Exercises, Speaking, Mistakes, Assessments, StudyPlans, LearningContent.

## Contracts used

- `Users.Contracts` — language settings, timezone, level.
- `Vocabulary.Contracts` / `Mistakes.Contracts` (if Contract Readers exist) — for "due now" counts surfaced as small numbers; otherwise read from event-fed projections in Progress' own schema.

## Main flow

This workflow is **read-side**. The write-side comes from many other workflows that emit events; Progress projects them into its own read models.

```
[Client] ──GET /progress/dashboard──> [Api]
   ↓ Progress query handler reads from progress.* projection tables (single module)
   ↓ assembles DashboardResponse (EXP, level, streak, today's targets, weekly chart,
       recent mistakes summary, due cards, recent achievements)
   ↓ return
```

The projections that back the dashboard are updated by event handlers:

```
Vocabulary events       → Progress: EXP delta, streak touch, daily/weekly snapshot
Exercises events        → Progress: same
Speaking events         → Progress: same
Mistakes events         → Progress: mistake counters / mastered counts
Assessments events      → Progress: EXP bonus, achievements
LearningContent events  → Progress: lesson completion
StudyPlans events       → Progress: target completion ratio
```

Each handler:
1. Check Inbox `(EventId, ConsumerName)` — return early if processed.
2. Open transaction.
3. Compute EXP delta from config.
4. Update `UserExperience`.
5. Touch streak: compute current calendar day from the user's timezone (Users.Contracts); compare to `LastActiveBusinessDate`; advance or reset.
6. Update snapshot rows (daily/weekly/monthly).
7. Insert Inbox row + commit.

## Events consumed

Most cross-module events listed above. See `../modules/progress.md` for the full list.

## Read models / projections updated

All within the `progress` schema:

- `UserExperience` aggregate (per user, per target language)
- `DailyProgressSnapshot`
- `WeeklyProgressSnapshot`
- `MonthlyProgressSnapshot`
- `SkillProgress` (vocab / grammar / listening / speaking)
- `Achievement` rows

## Failure / retry behavior

- Event handler exception → outbox retry with backoff → dead-letter after `MaxRetries`.
- Inbox guarantees no double-counting on re-delivery.
- Streak boundary errors (timezone misread) are guarded by the user-timezone Contract Reader — fall back to user's UI language country default if absent.

## Notes

- The dashboard endpoint **never queries other modules' tables**. All cross-module data is pre-projected in `progress`.
- Read models can be rebuilt from events via a Worker job (`ProjectionRebuildJob`). Useful after schema changes or to backfill on first deploy. The job must be idempotent.
- Detailed attempts (every speaking turn, every vocab review) are not in `progress` — those belong to the owner modules. Progress holds summaries / aggregates only.
