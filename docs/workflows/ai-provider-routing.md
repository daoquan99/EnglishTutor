# AI Provider Registry And Runtime Routing

## Overview

AI runtime routing lets Admin configure which provider/model handles text generation, TTS, STT, live audio, grading, pronunciation scoring, exercise generation, and lesson generation without changing caller modules.

## Main Flow

1. Admin registers providers and provider model capabilities in the AI module.
2. Admin configures runtime routes per `AiTaskType` and `AiCapabilityType`.
3. Caller modules invoke AI through `AI.Contracts` or AI application handlers.
4. AI resolves the active route and sends provider type, provider/model names, base URL, and API-key secret reference to the infrastructure client factory.
5. AI.Infrastructure resolves the raw API key through `ISecretProvider`.
6. AI.Infrastructure calls the selected external provider client.
7. AI logs requests in `ai.AiRequestLogs`.

## Detailed Steps

1. `POST /api/admin/ai/providers` creates `AiProvider`.
2. `PUT /api/admin/ai/providers/{providerName}/models` creates or updates `AiProviderModel`.
3. `PUT /api/admin/ai/routes/{taskType}` validates provider/model capability and writes `AiRuntimeRoute`.
4. Runtime handlers call `IAiRuntimeRouter.ResolveAsync`.
5. `IAiRuntimeRouter` prefers active `AiRuntimeRoute`.
6. If no active route exists, the resolver falls back to legacy `ModelRoutingRule` and seeded defaults.
7. `IAiClient` is implemented by `AiClientFactory`, which dispatches only to clients in AI.Infrastructure.
8. Admin endpoints are authorized by permission codes such as `ai.providers.manage`, not role names.
9. Raw API keys are never stored in the database. `ApiKeySecretName` points to configuration, user-secrets, env vars, or a future vault.

## Secret Resolution

- `AiProviders` in appsettings defines provider base URLs, default model codes, optional model aliases, and API-key secret names.
- Local development should store raw keys with `dotnet user-secrets`.
- Docker/production should provide raw keys through environment variables or secret manager integration.
- `ISecretProvider` reads from `IConfiguration`, so appsettings, user-secrets, and env vars follow the normal .NET configuration merge behavior.
- Legacy secret names such as `AI__OpenAI__ApiKey` are mapped to the new `AI_PROVIDERS:OPENAI:API_KEY` convention.

## Modules Involved

- AI owns provider registry, route resolution, prompt templates, request logs, and provider clients.
- Speaking, Vocabulary, Exercises, Assessments, and LearningContent must use AI.Contracts when they need AI features.
- AdminReports can later read usage/cost projections, not provider clients.

## Contracts Used

- `AI.Contracts.IEnglishCorrectionService`
- `AI.Contracts.IAiExerciseGenerator`
- `AI.Contracts.IAiLessonGenerator`
- `AI.Contracts.IAssessmentGradingService`
- `AI.Contracts.IAudioGenerationService`

## Events Published/Consumed

- None in Phase X.
- Future async AI jobs should use Outbox/Inbox before crossing module boundaries.

## Read Models/Projections Updated

- None in Phase X.
- Request execution writes `ai.AiRequestLogs`.

## Failure And Retry Behavior

- Admin route configuration fails fast when provider/model/capability is missing or disabled.
- Runtime resolver falls back to legacy routing when no active runtime route exists.
- Runtime client selection uses provider type first, then provider name or legacy model fallback.
- OpenAI/OpenAI-compatible and DeepSeek use the chat completions endpoint.
- Gemini uses the REST `models/{model}:generateContent` endpoint.
- Provider call retry policy is not implemented in Phase X; provider failures surface to the caller and are logged.
- Long-running or async AI jobs must be moved to Worker and backed by Outbox/Inbox in a later phase.
