# Vocabulary Flashcard Review Workflow

## Overview
User reviews vocabulary flashcards, practices pronunciation. Mastery tracked with spaced repetition.

## Main Flow
```
1. User → GET /api/vocabulary/today (get words due for review)
2. Vocabulary → returns items where NextReviewAtUtc <= UtcNow + new items
3. User → GET /api/vocabulary/{id}/study-card (get full card)
4. Vocabulary → returns word, translations (native language), examples, mastery scores
5. User → POST /api/vocabulary/{id}/review (submit review result)
6. Vocabulary → updates UserVocabularyMastery (score, next review via spaced repetition)
7. Vocabulary → saves VocabularyReviewedIntegrationEvent to Outbox
8. Worker → dispatches to Progress (EXP + activity log + skill progress)
9. (Optional) User → POST /api/vocabulary/{id}/pronunciation-attempts
10. Vocabulary → stores attempt with pronunciation scores
11. Vocabulary → saves VocabularyPronunciationPracticedIntegrationEvent to Outbox
12. Worker → dispatches to Mistakes (if score < threshold) + Progress
```

## Spaced Repetition
- NextReviewAtUtc calculated based on review history
- Status transitions: New → Learning → Reviewing → Mastered (or → Weak)

## Modules Involved
Vocabulary, Progress (consumer), Mistakes (consumer)

## Events Published
- `VocabularyReviewedIntegrationEvent` → Progress
- `VocabularyMasteredIntegrationEvent` → Progress
- `VocabularyPronunciationPracticedIntegrationEvent` → Progress, Mistakes
