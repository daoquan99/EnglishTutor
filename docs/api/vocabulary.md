# Vocabulary API

All endpoints require authentication.

## Endpoints

| Endpoint | Purpose |
| --- | --- |
| `GET /api/vocabulary/today?targetLanguageCode=en` | Get due and new vocabulary items. |
| `GET /api/vocabulary/{id}/study-card?targetLanguageCode=en&nativeLanguageCode=vi` | Get full study card with translations/examples. |
| `POST /api/vocabulary/{id}/review?targetLanguageCode=en` | Submit flashcard review. |
| `POST /api/vocabulary/{id}/pronunciation-attempts?targetLanguageCode=en` | Submit word pronunciation score. |
| `POST /api/vocabulary/examples/{exampleId}/pronunciation-attempts?targetLanguageCode=en` | Submit example sentence pronunciation score. |
| `POST /api/vocabulary/{id}/mark-mastered?targetLanguageCode=en` | Mark vocabulary mastered. |

## Request Bodies

Review:

```json
{ "isCorrect": true, "score": 90 }
```

Word pronunciation:

```json
{ "audioUrl": null, "recognizedText": "apple", "pronunciationScore": 85, "accuracyScore": 90, "fluencyScore": 80, "completenessScore": 88, "feedback": "Good pronunciation." }
```

Example pronunciation:

```json
{ "recognizedText": "I eat an apple.", "pronunciationScore": 82, "accuracyScore": 85, "fluencyScore": 80, "feedback": "Clear sentence." }
```

## Response Bodies

Review/mastery:

```json
{ "vocabularyItemId": "00000000-0000-0000-0000-000000000000", "masteryStatus": "Reviewing", "meaningMasteryScore": 75, "nextReviewAtUtc": "2026-05-19T00:00:00Z" }
```

Pronunciation attempt:

```json
{ "attemptId": "00000000-0000-0000-0000-000000000000", "pronunciationScore": 85, "accuracyScore": 90, "fluencyScore": 80, "completenessScore": 88, "attemptedAtUtc": "2026-05-18T00:00:00Z" }
```

## Validation Rules

- Review score and pronunciation scores are clamped/validated in the 0-100 range.
- Target language code is required.
- Vocabulary item/example target language must match `targetLanguageCode`.
- Study card requires target and native language codes.
- Recognized text and feedback are required for pronunciation attempts.

## Error Codes

- `Error.Validation`: invalid score, language/input, or target-language mismatch.
- `Error.NotFound`: vocabulary item, example, or mastery record not found.

## Application Flow

- Review creates mastery on first review, records attempt, updates spaced repetition, and writes Outbox event.
- Today vocabulary excludes all items that already have user mastery records unless they are due for review.
- `NextReviewAtUtc` is calculated by domain spaced repetition logic.
- Pronunciation attempts are owned by Vocabulary and publish pronunciation events.
- Optional seed data creates a small English vocabulary set with Vietnamese translations and example translations when `SeedData:Enabled=true`.

## Seed Data

- `SeedData:Enabled=false` by default.
- When enabled, API startup seeds at least 20 English vocabulary items.
- Seeded items include Vietnamese meanings, example sentences, and Vietnamese example translations.
- Existing `(TargetLanguageCode, Word, PartOfSpeech)` entries are skipped.

## Related Modules

- Progress consumes review/pronunciation events.
- Mistakes consumes pronunciation events for low-score mistake creation.

## Integration Events Produced

- `VocabularyReviewedIntegrationEvent`
- `VocabularyMasteredIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExampleSentencePronunciationPracticedIntegrationEvent`

## Integration Events Consumed

- None.

## Read Models/Projections Updated

- Vocabulary mastery records.
- Progress projections through events.
