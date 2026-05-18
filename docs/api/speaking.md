# Speaking API

All endpoints require authentication.

## Endpoints

| Endpoint | Purpose |
| --- | --- |
| `POST /api/speaking/sessions` | Start speaking session. |
| `GET /api/speaking/sessions` | List user sessions. |
| `GET /api/speaking/sessions/{id}` | Get session detail. |
| `POST /api/speaking/sessions/{id}/turns` | Add turn and receive correction. |
| `POST /api/speaking/sessions/{id}/complete` | Complete session and create summary. |
| `GET /api/speaking/sessions/{id}/summary` | Get session summary. |

## Request Bodies

Start session:

```json
{ "sessionType": "FreeTalk", "topic": "daily life" }
```

Add turn:

```json
{ "userText": "I goes to school yesterday." }
```

## Response Bodies

Session:

```json
{ "id": "00000000-0000-0000-0000-000000000000", "sessionType": "FreeTalk", "topic": "daily life", "status": "Active", "startedAtUtc": "2026-05-18T00:00:00Z" }
```

Turn correction:

```json
{ "turnId": "00000000-0000-0000-0000-000000000000", "originalText": "I goes...", "correctedText": "I went...", "overallScore": 90, "feedback": "Use past tense." }
```

## Validation Rules

- Session type must map to the Speaking domain enum.
- Turns can only be added to active sessions.
- Session owner must match current user.

## Error Codes

- `Error.NotFound`: session or summary not found.
- `Error.Validation`: invalid session transition or request.
- `Error.Unauthorized`: missing/invalid JWT.

## Application Flow

1. Start session reads language context from Users.Contracts.
2. Session stores immutable language snapshot.
3. Add turn calls AI.Contracts for correction.
4. Correction result is stored with serialized mistake details and `SpeakingTurnCorrectedIntegrationEvent` is written to Outbox.
5. Complete session calculates summary, including total mistakes from stored correction payloads, and writes `SpeakingSessionCompletedIntegrationEvent`.

## Related Modules

- Users supplies language settings.
- AI supplies correction.
- Mistakes consumes corrected-turn events.
- Progress consumes corrected-turn and completed-session events.

## Integration Events Produced

- `SpeakingSessionStartedIntegrationEvent`
- `SpeakingTurnCorrectedIntegrationEvent`
- `SpeakingSessionCompletedIntegrationEvent`

## Integration Events Consumed

- None.

## Read Models/Projections Updated

- Speaking session summaries.
- Mistake/progress projections through events.
