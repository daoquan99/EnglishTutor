# Mistake Review

## Overview

Mistakes owns detailed mistake cards and review state. It creates mistakes from producer events such as speaking correction and pronunciation scoring events.

## Main Flow

1. Mistakes consumes a correction or pronunciation event.
2. The consumer checks Inbox for idempotency.
3. Mistakes creates one or more `Mistake` records.
4. Mistakes publishes `MistakeCreatedIntegrationEvent` through its Outbox.
5. User reviews or masters a mistake.
6. Mistakes publishes review/mastery events for Progress.

## Detailed Steps

- Speaking correction events provide original/corrected text and mistake details.
- Vocabulary pronunciation events may create pronunciation mistakes when scores are below threshold.
- `Mistake.Review` moves the mistake to reviewed state and schedules the next review.
- `Mistake.MarkMastered` moves the mistake to mastered state.

## Modules Involved

- Mistakes
- Speaking
- Vocabulary
- Progress
- Worker

## Contracts Used

- `SpeakingTurnCorrectedIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `MistakeCreatedIntegrationEvent`
- `MistakeReviewedIntegrationEvent`
- `MistakeMasteredIntegrationEvent`

## Events Published/Consumed

- Consumed by Mistakes: speaking correction and vocabulary pronunciation events.
- Published by Mistakes: created, reviewed, and mastered events.
- Consumed by Progress: mistake review/mastery events.

## Read Models/Projections Updated

- Mistake review cards
- Progress dashboard mistake counts
- Progress activity logs

## Failure/Retry Behavior

- Mistakes consumers must use Inbox before creating mistake records.
- Mistakes producer events use Outbox.
- Failed events are retried by Worker and dead-lettered after retry limits.
