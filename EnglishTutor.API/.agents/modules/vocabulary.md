# Vocabulary module

Flashcards, spaced repetition state, mastery, and per-word practice attempts (pronunciation, fill-blank with example sentences).

## Schema

`vocabulary`

## Aggregate roots

| Aggregate                                  | Purpose                                                                                       |
| ------------------------------------------ | --------------------------------------------------------------------------------------------- |
| `VocabularyItem`                           | The catalog entry for a target-language word: lemma, translations, examples, audio.           |
| `UserVocabularyMastery`                    | Per-user, per-target-language mastery state: SRS interval, ease, next-review, correct count.  |
| `VocabularyPronunciationAttempt`           | A single pronunciation practice attempt for a word (score, transcript, AI feedback).          |
| `ExampleSentencePronunciationAttempt`      | Pronunciation practice for a specific example sentence.                                       |
| `ExampleFillBlankAttempt`                  | Fill-in-the-blank attempt against an example sentence.                                        |
| `VocabularyStudySettings`                  | Per-user preferences: daily new-card limit, review mix, audio autoplay, etc.                  |

## Contracts surface

- `IntegrationEvents/`:
  - `VocabularyReviewedIntegrationEvent` — every SRS review.
  - `VocabularyMasteredIntegrationEvent` — when mastery threshold reached.
  - `VocabularyPronunciationPracticedIntegrationEvent`
  - `ExampleSentencePronunciationPracticedIntegrationEvent`
  - `ExampleFillBlankAttemptedIntegrationEvent`

  All consumed by Progress (EXP), Mistakes (on incorrect), StudyPlans (target completion).

## Key behaviors

- SRS algorithm lives in `Domain/UserVocabularyMastery/`. Pure domain logic — no infra deps.
- `UserVocabularyMastery` is scoped by `UserId + VocabularyItemId + TargetLanguageCode`.
- Pronunciation scoring uses `AI.Contracts` (no direct provider calls).
- Repository: `IUserVocabularyMasteryRepository` (per aggregate root, as the rule).

## Notes for changes

- Adding a practice mode: introduce a new attempt aggregate + integration event; do not extend an existing attempt type with discriminator hacks.
- Mastery threshold / SRS tuning: changes belong in the domain class, behind named business rules.
- Don't query LearningContent's lesson tables to pull example sentences — use the `LearningContent.Contracts` reader.
