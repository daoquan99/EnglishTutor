# AI API

AI owns provider registry, provider clients, prompt building, runtime model routing, request logging, and contract implementations.

## Endpoints

| Endpoint | Purpose | Auth |
| --- | --- | --- |
| `POST /api/ai/correct-sentence` | Correct a sentence using AI runtime routing and prompt context. | User |
| `GET /api/admin/ai/providers` | List configured AI providers and their models. | Permission: `ai.providers.read` |
| `POST /api/admin/ai/providers` | Register a provider registry entry. | Permission: `ai.providers.manage` |
| `PUT /api/admin/ai/providers/{providerName}/models` | Add or update a model/capability under a provider. | Permission: `ai.providers.manage` |
| `GET /api/admin/ai/routes` | List runtime task routes. | Permission: `ai.routes.read` |
| `PUT /api/admin/ai/routes/{taskType}` | Configure runtime routing for a task/capability. | Permission: `ai.routes.manage` |
| `GET /api/admin/ai/routes/{taskType}/resolve?capability=TextGeneration` | Resolve the effective route, including legacy fallback. | Permission: `ai.routes.read` |

## Correct Sentence

### Endpoint

`POST /api/ai/correct-sentence`

### Purpose

Correct learner text using prompt templates and the active AI runtime route for `SentenceCorrection`.

### Auth Requirement

Authenticated user.

### Request Body

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

### Response Body

```json
{
  "isSuccess": true,
  "data": {
    "originalText": "I goes to school yesterday.",
    "correctedText": "I went to school yesterday.",
    "naturalVersion": "I went to school yesterday.",
    "grammarScore": 80,
    "vocabularyScore": 90,
    "feedback": "Use past tense for yesterday.",
    "mistakes": []
  },
  "error": null
}
```

### Validation Rules

- `originalText` is required.
- Language context must include native, target, and explanation language codes.
- User level should match the shared language-level vocabulary.

### Error Codes

- `Error.Validation`: invalid request.
- `Error.Unauthorized`: missing/invalid JWT.
- Provider failures are surfaced as server errors until provider-specific retry policy is expanded.

### Application Flow

1. Resolve the active `AiRuntimeRoute` for `SentenceCorrection` and `TextGeneration`.
2. If no runtime route exists, fall back to the legacy `ModelRoutingRule` default.
3. Build the prompt from the active `PromptTemplate`/`PromptVersion`, with seeded defaults as fallback.
4. Call `IAiClient`; `AiClientFactory` routes the request by provider/model metadata.
5. Save `AiRequestLog` with the current user id.
6. Parse structured JSON fields from the provider response: `correctedText`, `naturalVersion`, `grammarScore`, `vocabularyScore`, `feedback`, and `mistakes`.
7. If the provider response is not structured JSON, return the raw correction text with zero scores and a fallback feedback message instead of treating the answer as perfect.

### Related Modules

- Speaking uses `AI.Contracts.IEnglishCorrectionService`.
- Other modules must use `AI.Contracts`; provider clients remain inside AI.Infrastructure.

### Integration Events Produced

- None.

### Integration Events Consumed

- None.

### Read Models/Projections Updated

- `ai.AiRequestLogs` for usage/cost analysis.

## Assessment Grading Contract

Exercises calls `AI.Contracts.IAssessmentGradingService` for AI-graded exercise questions only. The AI module prompts the selected provider to return strict JSON:

```json
{
  "score": 82,
  "feedback": "Good answer. Fix article usage.",
  "rubricScores": {
    "overall": 82
  }
}
```

The parser rejects non-JSON, missing score, missing feedback, and scores outside `0..100`. Invalid provider output is logged as a failed AI request and surfaces as a server-side grading failure; it is not converted into a default passing score.

## Admin Provider Registry

### Endpoint

`GET /api/admin/ai/providers`

### Purpose

List provider registry entries and their configured model capabilities.

### Auth Requirement

Authenticated user with `ai.providers.read` or `admin.full_access`.

### Response Body

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "providerName": "google",
      "displayName": "Google AI",
      "providerType": "Google",
      "baseUrl": null,
      "apiKeySecretName": "AI_PROVIDERS:GOOGLE:API_KEY",
      "isEnabled": true,
      "models": [
        {
          "id": "00000000-0000-0000-0000-000000000000",
          "modelCode": "gemini-flash",
          "displayName": "Gemini Flash",
          "capability": "TextGeneration",
          "isEnabled": true,
          "supportsStreaming": true,
          "maxInputTokens": 1000000,
          "maxOutputTokens": 8192,
          "costPerInput1KTokens": 0,
          "costPerOutput1KTokens": 0,
          "priority": 10
        }
      ]
    }
  ],
  "error": null
}
```

### Endpoint

`POST /api/admin/ai/providers`

### Purpose

Register a provider without storing raw API keys in the database.

### Request Body

```json
{
  "providerName": "openai",
  "displayName": "OpenAI",
  "providerType": "OpenAI",
  "baseUrl": null,
  "apiKeySecretName": "AI_PROVIDERS:OPENAI:API_KEY",
  "isEnabled": false
}
```

### Validation Rules

- `providerName` is required, unique, lowercase-normalized, and may contain only letters, digits, hyphen, or underscore.
- `providerType` must match `Google`, `OpenAI`, `DeepSeek`, `OpenAiCompatible`, `Local`, or `Custom`.
- `displayName` is required.
- `apiKeySecretName` stores the configuration/secret reference, not the raw API key.
- Raw API keys must be stored in dotnet user-secrets, environment variables, or a future secret manager.
- Appsettings maps provider base URLs and secret names under `AiProviders`; it must not contain raw API keys.

### Error Codes

- `Error.Validation`: invalid provider type or invalid field values.
- `Error.Conflict`: provider already exists.
- `Error.Unauthorized`: missing/invalid JWT.
- `Error.Forbidden`: authenticated user lacks the required permission.

### Endpoint

`PUT /api/admin/ai/providers/{providerName}/models`

### Purpose

Add or update a provider model for one capability.

### Request Body

```json
{
  "modelCode": "gemini-flash",
  "displayName": "Gemini Flash",
  "capability": "TextGeneration",
  "supportsStreaming": true,
  "maxInputTokens": 1000000,
  "maxOutputTokens": 8192,
  "costPerInput1KTokens": 0,
  "costPerOutput1KTokens": 0,
  "priority": 10,
  "isEnabled": true
}
```

### Validation Rules

- Provider must exist.
- `capability` must match a supported `AiCapabilityType`.
- Token limits must be positive.
- Costs must not be negative.
- A provider can register the same model code for multiple capabilities.

### Application Flow

1. Load provider by name.
2. Add a new model/capability or update the existing one.
3. Save changes in the AI schema.

## Admin Runtime Routes

### Endpoint

`GET /api/admin/ai/routes`

### Purpose

List configured runtime routes.

### Endpoint

`PUT /api/admin/ai/routes/{taskType}`

### Purpose

Configure which provider/model handles a task and capability at runtime.

### Request Body

```json
{
  "capability": "TextGeneration",
  "preferredProviderName": "google",
  "preferredModelCode": "gemini-flash",
  "fallbackProviderName": "local",
  "fallbackModelCode": "gemma",
  "maxTokens": 2048,
  "temperature": 0.2,
  "isActive": true
}
```

### Response Body

```json
{
  "isSuccess": true,
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "taskType": "SentenceCorrection",
    "capability": "TextGeneration",
    "preferredProviderName": "google",
    "preferredModelCode": "gemini-flash",
    "fallbackProviderName": "local",
    "fallbackModelCode": "gemma",
    "maxTokens": 2048,
    "temperature": 0.2,
    "isActive": true
  },
  "error": null
}
```

### Validation Rules

- `taskType` must match `AiTaskType`.
- `capability` must match `AiCapabilityType`.
- Preferred provider/model must exist and be enabled for the selected capability.
- Fallback provider/model must be configured together.
- Fallback provider/model must exist and be enabled when provided.
- `maxTokens` must be positive.
- `temperature` must be between `0` and `2`.

### Error Codes

- `Error.Validation`: invalid task/capability, disabled provider/model, or invalid route settings.
- `Error.NotFound`: provider or model was not found.
- `Error.Unauthorized`: missing/invalid JWT.
- `Error.Forbidden`: authenticated user lacks the required permission.

### Endpoint

`GET /api/admin/ai/routes/{taskType}/resolve?capability=TextGeneration`

### Purpose

Show the effective route that runtime execution will use. If no active runtime route exists, the resolver returns the legacy default route.

### Application Flow

1. Query active `AiRuntimeRoute` by task and capability.
2. If found, return its provider/model settings.
3. If not found, fall back to `ModelRoutingRule` and seeded defaults.
4. Runtime handlers pass provider/model metadata to `IAiClient`.
5. `AiClientFactory` chooses the infrastructure client by provider type first, then provider name or legacy model fallback.
6. Provider clients resolve the raw key through `ISecretProvider`.

## Provider Secrets

For local development, initialize user-secrets on the API host:

```bash
dotnet user-secrets init --project src/Bootstrapper/EnglishTutor.Api
dotnet user-secrets set "AI_PROVIDERS:GOOGLE:API_KEY" "<gemini-key>" --project src/Bootstrapper/EnglishTutor.Api
dotnet user-secrets set "AI_PROVIDERS:OPENAI:API_KEY" "<openai-key>" --project src/Bootstrapper/EnglishTutor.Api
dotnet user-secrets set "AI_PROVIDERS:DEEPSEEK:API_KEY" "<deepseek-key>" --project src/Bootstrapper/EnglishTutor.Api
```

Docker/production uses equivalent environment variables:

```bash
AI_PROVIDERS__GOOGLE__API_KEY=<gemini-key>
AI_PROVIDERS__OPENAI__API_KEY=<openai-key>
AI_PROVIDERS__DEEPSEEK__API_KEY=<deepseek-key>
```

`AiProviders` appsettings only contains non-secret provider configuration:

```json
{
  "AiProviders": {
    "OpenAI": {
      "BaseUrl": "https://api.openai.com/v1",
      "ApiKeySecretName": "AI_PROVIDERS:OPENAI:API_KEY",
      "DefaultModelCode": "gpt-4o-mini"
    }
  }
}
```

## Provider Clients

- `OpenAiCompatibleClient` calls `POST /chat/completions` with bearer auth.
- `DeepSeekClient` uses the OpenAI-compatible client with DeepSeek base URL and API key.
- `GeminiClient` calls `POST /v1beta/models/{model}:generateContent` with `x-goog-api-key`.
- `GemmaClient` remains a local boundary until a real local runtime endpoint is configured.

### Related Modules

- All caller modules use AI through `AI.Contracts`.
- Provider registry and runtime routes are owned only by AI.

### Integration Events Produced

- None.

### Integration Events Consumed

- None.

### Read Models/Projections Updated

- None.

## Seed Data

- `SeedData:Enabled=false` by default, so startup does not require a live database while the project is still under construction.
- When enabled, API startup seeds default `AiProvider`, `AiProviderModel`, `AiRuntimeRoute`, `ModelRoutingRule`, and prompt template rows.
- Existing providers, routes, routing rules, or prompt templates are not overwritten, so Admin configuration owns provider/model selection safely.
