# Vocabulary Flashcard Review

## Overview

Vocabulary owns detailed vocabulary study attempts and mastery state. Progress owns summaries and dashboards that react to vocabulary integration events.

## Main Flow

1. Vocabulary loads due items for the user's active target language.
2. User reviews a vocabulary item.
3. Vocabulary records `VocabularyReviewAttempt`.
4. `UserVocabularyMastery.RecordReview` updates score, status, and next review date.
5. Vocabulary maps domain events to integration events and saves OutboxMessages.
6. Worker dispatches events to Progress and other consumers.
7. Consumers use Inbox for idempotency.

## Detailed Steps

- `VocabularyReviewedIntegrationEvent` is published after each review.
- `VocabularyMasteredIntegrationEvent` is published when status reaches mastered or the user marks the item mastered.
- `VocabularyPronunciationPracticedIntegrationEvent` and `ExampleSentencePronunciationPracticedIntegrationEvent` are published after pronunciation scoring.
- Progress updates EXP, skill progress, activity logs, streaks, and dashboard snapshots from the events.
- Mistakes may create pronunciation mistake cards from low pronunciation scores.

## Modules Involved

- Vocabulary
- Progress
- Mistakes
- Worker

## Contracts Used

- `VocabularyReviewedIntegrationEvent`
- `VocabularyMasteredIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExampleSentencePronunciationPracticedIntegrationEvent`

## Events Published/Consumed

- Published by Vocabulary: vocabulary review, mastery, and pronunciation events.
- Consumed by Progress: review, mastery, and pronunciation events.
- Consumed by Mistakes: pronunciation events when mistake extraction is needed.

## Read Models/Projections Updated

- Progress activity logs
- Progress skill summaries
- Progress dashboard snapshots
- Mistake review cards when low pronunciation scores are detected

## Failure/Retry Behavior

- Vocabulary saves events through its own Outbox.
- Worker retries failed dispatch.
- Consumers must process events idempotently through Inbox.
