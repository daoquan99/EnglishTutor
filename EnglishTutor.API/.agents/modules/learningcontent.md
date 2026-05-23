# LearningContent module

Authored learning material: lessons, conversation scenarios, sentence patterns, quizzes. The "library" the platform draws from.

## Schema

`learningcontent`

## Aggregate roots

| Aggregate                | Purpose                                                                          |
| ------------------------ | -------------------------------------------------------------------------------- |
| `Lesson`                 | Authored lesson unit: title, level, language, blocks, attached vocab/exercises.  |
| `ConversationScenario`   | Scripted multi-turn dialog used by Speaking.                                     |
| `SentencePattern`        | Grammar pattern with example sentences and template slots.                       |
| `Quiz`                   | Short comprehension quiz attached to a lesson.                                   |
| `UserLearningPathCard`   | A user's progression marker through the lesson tree (per target language).       |

## Contracts surface

- `Readers/` — Contract Reader interfaces to fetch lesson summaries, conversation scenarios for Speaking, sentence patterns for Vocabulary examples.
- `ReadModels/` — DTOs returned by readers.
- `IntegrationEvents/`:
  - `LessonPublishedIntegrationEvent`
  - `LessonCompletedIntegrationEvent` — consumed by Progress.
  - `ConversationScenarioCompletedIntegrationEvent` — consumed by Progress.

## Key behaviors

- Lesson content supports translations per `NativeLanguageCode` (example sentences, hints, instructions).
- `UserLearningPathCard` is a small per-user aggregate; the broader "learning path" view is a Read Model in the consuming UI module's schema (built from these events + Progress).
- Authoring/publishing distinguishes draft vs published. Only published content surfaces to learners.

## Notes for changes

- Adding a lesson block type: extend the aggregate's block list with a discriminated subtype; bump reader DTO version.
- Content translations: add per-language fields to translation tables; do not denormalize translations onto the main lesson row.
