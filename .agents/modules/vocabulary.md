# Vocabulary Module

## Responsibility
Owns vocabulary flashcards, translations, examples, detailed attempts, mastery tracking.

## Schema
`vocabulary`

## Tables
- `vocabulary.VocabularyItems` — Word, Phonetic, Level, Topic, PartOfSpeech
- `vocabulary.VocabularyTranslations` — Meaning, ShortExplanation per language
- `vocabulary.VocabularyExamples` — Example sentences per word
- `vocabulary.VocabularyExampleTranslations` — Translations per example
- `vocabulary.UserVocabularyProgress` — User-specific progress
- `vocabulary.UserVocabularyMastery` — MeaningMasteryScore, PronunciationMasteryScore, Status (New/Learning/Reviewing/Weak/Mastered)
- `vocabulary.VocabularyReviewSessions` — Review session tracking
- `vocabulary.VocabularyReviewAttempts` — Individual review attempts
- `vocabulary.VocabularyPronunciationAttempts` — Word pronunciation with scores
- `vocabulary.ExampleSentencePronunciationAttempts` — Sentence pronunciation with word-level feedback
- `vocabulary.UserVocabularyStudyCards` — Read model for study cards

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/vocabulary/today` | Yes |
| GET | `/api/vocabulary/{id}/study-card` | Yes |
| POST | `/api/vocabulary/{id}/review` | Yes |
| POST | `/api/vocabulary/{id}/pronunciation-attempts` | Yes |
| POST | `/api/vocabulary/examples/{exampleId}/pronunciation-attempts` | Yes |
| POST | `/api/vocabulary/{id}/mark-mastered` | Yes |

## Events Produced
- `VocabularyReviewedIntegrationEvent`
- `VocabularyMasteredIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExampleSentencePronunciationPracticedIntegrationEvent`
