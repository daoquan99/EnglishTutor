# Level-Up Assessment Workflow

## Overview

Assessments owns assessment attempts and pass/fail rules. AI only grades rubric-based answers. Users owns the learner's current level. Progress, LearningContent, and Notifications react to the eventual `UserLevelChangedIntegrationEvent`.

## Main Flow

1. Learner starts a level-up assessment.
2. Learner submits answers.
3. Assessments combines static grading and AI grading.
4. Domain pass rule decides passed or failed.
5. Passed attempts publish level-up approval through outbox.
6. Users consumes level-up approval and updates `UserTargetLanguage`.
7. Users publishes `UserLevelChangedIntegrationEvent`.
8. Progress grants EXP and updates dashboard snapshot.
9. LearningContent refreshes learning path cards.
10. Notifications creates a congratulations notification.

## Detailed Steps

- `StartLevelUpAssessmentCommandHandler` reads target language through `Users.Contracts.IUserTargetLanguageReader`.
- Eligibility blocks repeated failed attempts inside the cooldown window.
- `SubmitAssessmentCommandHandler` requires all answers before grading.
- AI grading returns strict JSON score and feedback through `AI.Contracts.IAssessmentGradingService`.
- `UserAssessmentAttempt.ApplyGradingResult` applies the domain pass rule.
- `AssessmentsDbContext` maps assessment domain events to integration events and writes outbox rows in the same transaction.
- Worker dispatches events at least once; every consumer uses Inbox idempotency.

## Modules Involved

Assessments, AI, Users, Progress, LearningContent, Notifications, Worker, Messaging.

## Contracts Used

- `Users.Contracts.IUserTargetLanguageReader`
- `AI.Contracts.IAssessmentGradingService`
- `Assessments.Contracts.IntegrationEvents`
- `Users.Contracts.IntegrationEvents.UserLevelChangedIntegrationEvent`

## Events Published/Consumed

- Published by Assessments: `AssessmentStartedIntegrationEvent`, `AssessmentCompletedIntegrationEvent`, `AssessmentPassedIntegrationEvent`, `AssessmentFailedIntegrationEvent`, `LevelUpApprovedIntegrationEvent`.
- Consumed by Users: `LevelUpApprovedIntegrationEvent`.
- Published by Users: `UserLevelChangedIntegrationEvent`.
- Consumed by Progress, LearningContent, Notifications: `UserLevelChangedIntegrationEvent`.

## Read Models/Projections Updated

- Progress dashboard snapshot and activity logs.
- LearningContent learning path cards.
- Notification messages.
- AdminReports assessment and user overview projections when reporting handlers are enabled.

## Failure/Retry Behavior

Outbox delivery is at-least-once. Consumers check Inbox before processing. Failed dispatches retry through Worker and move to dead-letter after retry limits.
