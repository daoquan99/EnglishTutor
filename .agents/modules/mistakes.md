# Mistakes Module

## Responsibility
Owns user mistakes and mistake review queue.

## Schema
`mistakes`

## Tables
- `mistakes.Mistakes` — UserId, Type, Category, OriginalText, CorrectedText, Explanation, SourceType, SourceId, Status, NextReviewAtUtc
- `mistakes.MistakeReviews` — Review tracking
- `mistakes.MistakeCategories` — Mistake categorization
- `mistakes.UserMistakeCards` — Read model

## SourceType (where mistake came from)
SpeakingTurn, VocabularyPronunciationAttempt, ExampleSentencePronunciationAttempt, ExerciseAnswer, AssessmentAnswer, ConversationTurn

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/mistakes` | Yes |
| GET | `/api/mistakes/today` | Yes |
| GET | `/api/mistakes/{id}` | Yes |
| POST | `/api/mistakes/{id}/review` | Yes |
| POST | `/api/mistakes/{id}/mark-mastered` | Yes |

## Events Consumed
- `SpeakingTurnCorrectedIntegrationEvent` → create mistakes
- `ExerciseCompletedIntegrationEvent` → create mistakes from wrong answers
- `VocabularyPronunciationPracticedIntegrationEvent` → create pronunciation mistakes
- `ExampleSentencePronunciationPracticedIntegrationEvent`
- `AssessmentCompletedIntegrationEvent`

## Events Produced
- `MistakeCreatedIntegrationEvent`
- `MistakeReviewedIntegrationEvent`
- `MistakeMasteredIntegrationEvent`
