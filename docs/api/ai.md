# AI API

AI owns provider clients, prompt building, model routing, request logging, and contract implementations.

## Endpoints

| Endpoint | Purpose | Auth |
| --- | --- | --- |
| `POST /api/ai/correct-sentence` | Correct a sentence using AI routing and prompt context. | Required |

## Request Body

```json
{
  "originalText": "I goes to school yesterday.",
  "nativeLanguageCode": "vi",
  "targetLanguageCode": "en",
  "explanationLanguageCode": "vi",
  "userLevel": "A1",
  "topic": "daily life"
}
```

## Response Body

```json
{
  "originalText": "I goes to school yesterday.",
  "correctedText": "I went to school yesterday.",
  "naturalVersion": "I went to school yesterday.",
  "grammarScore": 80,
  "vocabularyScore": 90,
  "feedback": "Use past tense for yesterday.",
  "mistakes": []
}
```

## Validation Rules

- `originalText` is required.
- Language context must include native, target, and explanation language codes.
- User level should match the shared language-level vocabulary.

## Error Codes

- `Error.Validation`: invalid request.
- `Error.Unauthorized`: missing/invalid JWT.
- Provider failures are surfaced as server errors until provider-specific retry policy is expanded.

## Application Flow

1. Resolve the active `ModelRoutingRule` for `SentenceCorrection` from AI storage, with seeded defaults as fallback.
2. Build the prompt from the active `PromptTemplate`/`PromptVersion`, with seeded defaults as fallback.
3. Call `IAiClient`; `AiClientFactory` routes the request to the provider client selected by the model rule.
4. Save `AiRequestLog` with the current user id.
5. Return correction DTO.

## Related Modules

- Speaking uses `AI.Contracts.IEnglishCorrectionService`.
- `AI.Contracts.CorrectionRequest` includes the caller's `UserId` so AI can persist request logs without reading another module's context directly.
- Other modules must not call Gemini/Gemma clients directly; provider clients remain inside AI.Infrastructure.

## Integration Events Produced

- None.

## Integration Events Consumed

- None.

## Read Models/Projections Updated

- `AiRequestLog` for usage/cost analysis.
