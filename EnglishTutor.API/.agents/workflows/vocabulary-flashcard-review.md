# Workflow — Vocabulary flashcard review

User reviews due flashcards using a spaced-repetition algorithm. May include pronunciation and fill-blank practice on example sentences.

## Modules involved

Vocabulary (owner), Users, LearningContent (example sentences), AI (pronunciation scoring, fill-blank grading for free-form), Mistakes, Progress, StudyPlans, AdminReports.

## Contracts used

- `Users.Contracts` — target language + level.
- `LearningContent.Contracts` — pull example sentences attached to a `VocabularyItem`.
- `AI.Contracts` — pronunciation scoring and free-form fill-blank grading.

## Main flow

```
[Client] ──GET /vocabulary/today──> [Api]
   ↓ list due UserVocabularyMastery rows (UserId + TargetLanguageCode)
   ↓ enrich with VocabularyItem (Vocabulary's own data) and example sentences (LearningContent.Contracts)
   ↓ return study deck

[Client] ──POST /vocabulary/items/{id}/review {grade}──> [Api]
   ↓ load UserVocabularyMastery aggregate
   ↓ apply SRS algorithm (domain method), set NextReviewAtUtc, EaseFactor, IntervalDays
   ↓ if mastery threshold crossed: raise VocabularyMastered domain event
   ↓ OutboxMessage: VocabularyReviewedIntegrationEvent
   ↓ on mastery: OutboxMessage: VocabularyMasteredIntegrationEvent
   ↓ if grade was 'incorrect' → consumed by Mistakes via the event

[Client] (optional) ──POST /vocabulary/items/{id}/pronounce──> [Api]
   ↓ upload audio
   ↓ AI.Contracts: score pronunciation
   ↓ append VocabularyPronunciationAttempt
   ↓ OutboxMessage: VocabularyPronunciationPracticedIntegrationEvent

[Client] (optional) ──POST /vocabulary/items/{id}/example-fill-blank──> [Api]
   ↓ grade via Vocabulary domain (deterministic) or AI.Contracts (free-form)
   ↓ append ExampleFillBlankAttempt
   ↓ OutboxMessage: ExampleFillBlankAttemptedIntegrationEvent

[Worker]
   ↓ Mistakes: upsert Mistake on incorrect reviews / wrong fill-blank
   ↓ Progress: EXP + streak from any reviewed/practiced event
   ↓ StudyPlans: mark daily new-card / review targets
   ↓ AdminReports: LearningActivityReport, CommonMistakeStat
```

## Events published

- `VocabularyReviewedIntegrationEvent`
- `VocabularyMasteredIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExampleSentencePronunciationPracticedIntegrationEvent`
- `ExampleFillBlankAttemptedIntegrationEvent`

## Read models / projections updated

- Progress: EXP, streak, daily/weekly snapshot.
- Mistakes: `Mistake` rows on incorrect / wrong-fill-blank.
- StudyPlans: daily-target counters.
- AdminReports: `LearningActivityReport`, `CommonMistakeStat`.

## Failure / retry behavior

- AI scoring failure: pronunciation/fill-blank attempt not recorded; client retries.
- The SRS update itself is deterministic and doesn't depend on AI — if AI fails on a fill-blank, the underlying review state was not changed.
- Outbox retry standard.

## Notes

- `UserVocabularyMastery` scoping: `UserId + VocabularyItemId + TargetLanguageCode`. Same word in a different target language is a different mastery row.
- Daily new-card limit is in `VocabularyStudySettings`.
- `Vocabulary` never queries LearningContent's tables directly; example sentences come through `LearningContent.Contracts`.
