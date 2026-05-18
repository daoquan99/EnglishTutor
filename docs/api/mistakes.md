# Mistakes API

All endpoints require authentication.

## Endpoints

| Endpoint | Purpose |
| --- | --- |
| `GET /api/mistakes` | List user mistakes. |
| `GET /api/mistakes/today` | List mistakes due for review. |
| `GET /api/mistakes/{id}` | Get mistake detail. |
| `POST /api/mistakes/{id}/review` | Mark mistake reviewed and schedule next review. |
| `POST /api/mistakes/{id}/mark-mastered` | Mark mistake mastered. |

## Request Body

Review and mark-mastered currently use route parameters only.

## Response Body

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "targetLanguageCode": "en",
  "type": "Grammar",
  "category": "Tense",
  "originalText": "I goes",
  "correctedText": "I go",
  "explanation": "Use base verb with I.",
  "sourceType": "SpeakingTurn",
  "sourceId": "00000000-0000-0000-0000-000000000000",
  "status": "Reviewed",
  "nextReviewAtUtc": "2026-05-19T00:00:00Z",
  "createdAtUtc": "2026-05-18T00:00:00Z"
}
```

## Validation Rules

- Current user must own the mistake.
- Mastered mistakes should not be reviewed again unless domain policy changes.

## Error Codes

- `Error.NotFound`: mistake not found or belongs to another user.
- `Error.Validation`: invalid mistake state.

## Application Flow

- Mistakes are normally created by event handlers from Speaking and Vocabulary events.
- Review/mastery commands update mistake state and write Mistakes Outbox events.
- Inbox prevents duplicate processing of producer events.

## Related Modules

- Speaking produces corrected-turn events.
- Vocabulary produces pronunciation events.
- Progress consumes mistake review events.

## Integration Events Produced

- `MistakeCreatedIntegrationEvent`
- `MistakeReviewedIntegrationEvent`
- `MistakeMasteredIntegrationEvent`

## Integration Events Consumed

- `SpeakingTurnCorrectedIntegrationEvent`
- `VocabularyPronunciationPracticedIntegrationEvent`
- `ExampleSentencePronunciationPracticedIntegrationEvent`

## Read Models/Projections Updated

- Mistake review cards.
- Progress projections through events.
