# LearningContent API

## Overview

LearningContent owns published lessons, conversation scenarios, sentence patterns, and user learning path cards.

## Endpoints

### `GET /api/lessons`
- Purpose: List published lessons.
- Auth requirement: Authenticated user.
- Request query: `page`, `pageSize`, `level`, `topic`, `skill`, `targetLanguageCode`.
- Response body: list of `LessonListResponse`.
- Validation rules: page defaults to 1, page size defaults to 20 and is capped at 100.
- Application flow: query published lessons, translate title/description using user's UI language when available.
- Related modules: Users.

### `GET /api/lessons/{id}`
- Purpose: Get lesson detail with ordered sections.
- Auth requirement: Authenticated user.
- Response body: `LessonDetailResponse`.
- Error codes: `Error.NotFound`, `Error.Validation` when content is unpublished.

### `POST /api/lessons/{id}/complete`
- Purpose: Mark a lesson path card as completed.
- Auth requirement: Authenticated user.
- Request query: optional `durationSeconds`.
- Response body: updated `LearningPathCardResponse`.
- Application flow: validate lesson is published, complete/create card, unlock next card, save outbox event.
- Integration events produced: `LessonCompletedIntegrationEvent`.

### `GET /api/learning-path`
- Purpose: Get current user's learning path cards.
- Auth requirement: Authenticated user.
- Request query: optional `targetLanguageCode`.
- Response body: ordered list of `LearningPathCardResponse`.

### `GET /api/conversations`
- Purpose: List published conversation scenarios.
- Auth requirement: Authenticated user.
- Request query: `page`, `pageSize`, `level`, `difficulty`, `targetLanguageCode`.
- Response body: list of `ConversationListResponse`.

### `GET /api/conversations/{id}`
- Purpose: Get conversation scenario detail with ordered lines.
- Auth requirement: Authenticated user.
- Response body: `ConversationDetailResponse`.
- Error codes: `Error.NotFound`, `Error.Validation` when content is unpublished.

## Contracts And Events

- Contract reader: `IConversationScenarioReader` for Speaking conversation practice.
- Events produced: `LessonPublishedIntegrationEvent`, `LessonCompletedIntegrationEvent`, `ConversationScenarioCompletedIntegrationEvent`.
- Events consumed: `UserLevelChangedIntegrationEvent`, `SpeakingSessionCompletedIntegrationEvent`.
