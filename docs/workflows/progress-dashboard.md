# Progress Dashboard

## Overview

Progress stores summaries, aggregates, activity logs, EXP, streaks, skill progress, and dashboard snapshots. It does not own detailed attempts from Vocabulary, Speaking, Exercises, Assessments, or Mistakes.

## Main Flow

1. Producer modules publish integration events for completed learning actions.
2. Progress consumers check Inbox.
3. Progress creates `LearningActivityLog` records.
4. Progress grants EXP and updates rank.
5. Progress updates skill progress, daily/weekly/monthly summaries, streaks, and dashboard snapshots.

## Detailed Steps

- Vocabulary review events update vocabulary skill and EXP.
- Vocabulary and example-sentence pronunciation events update pronunciation skill.
- Speaking corrected-turn events update speaking, grammar, and vocabulary skill scores.
- Speaking completion events update speaking, grammar, vocabulary, pronunciation, streak, and dashboard data.
- Mistake review events update activity logs and EXP.
- Dashboard queries should read precomputed snapshots, not live cross-module joins.

## Modules Involved

- Progress
- Vocabulary
- Speaking
- Mistakes
- Worker

## Contracts Used

- `VocabularyReviewedIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExampleSentencePronunciationPracticedIntegrationEvent`
- `SpeakingTurnCorrectedIntegrationEvent`
- `SpeakingSessionCompletedIntegrationEvent`
- `MistakeReviewedIntegrationEvent`

## Events Published/Consumed

- Progress consumes module events and does not query producer DbContexts.

## Read Models/Projections Updated

- `LearningActivityLog`
- `UserExperience`
- `UserSkillProgress`
- `UserDailyProgress`
- `UserWeeklyProgress`
- `UserMonthlyProgress`
- `UserDashboardSnapshot`
- `UserStreak`

## Failure/Retry Behavior

- Progress event handlers must be idempotent through Inbox.
- Worker retries failed messages.
- Messages that exceed retry limits go to DeadLetterMessages.
