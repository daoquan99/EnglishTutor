# Exercises module

Multi-form grammar / comprehension exercises: multiple choice, fill-blank, translation, conjugation, ordering.

## Schema

`exercises`

## Aggregate roots

| Aggregate              | Purpose                                                                                  |
| ---------------------- | ---------------------------------------------------------------------------------------- |
| `ExerciseSet`          | Authored exercise bundle: questions, type, level, attached lesson/skill.                 |
| `UserExerciseAttempt`  | A user's attempt at an exercise set: per-question answers, score, time taken, mistakes.  |

## Contracts surface

- `IntegrationEvents/`:
  - `ExerciseStartedIntegrationEvent`
  - `ExerciseQuestionAnsweredIntegrationEvent` — per question, used by Mistakes (on incorrect) and AI feedback flow.
  - `ExerciseCompletedIntegrationEvent` — consumed by Progress, StudyPlans (daily target).

## Key behaviors

- Question types live in `Domain/Shared/` as a discriminated value object set (`MultipleChoiceQuestion`, `FillBlankQuestion`, `TranslationQuestion`, `ConjugationQuestion`, `OrderingQuestion`).
- Grading is per-question, in-aggregate. AI grading (for translation/free-response) is invoked through `AI.Contracts`.
- Mistakes are emitted per incorrect answer via the event above; the `Mistakes` module consumes and aggregates.

## Notes for changes

- Adding a new question type: add the value-object subtype + grading rule in Domain; extend the integration event payload (`QuestionType` discriminator); update `docs/api/exercises.md` and the FE renderer.
- Don't store the user's mistake list inside `UserExerciseAttempt` aside from per-question wrongness — Mistakes module owns the aggregated review state.
