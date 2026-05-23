# AI module

Central intelligence service. Owns all AI-provider interaction. See also `../rules/ai.md`.

## Schema

`ai`

## Aggregate roots

| Aggregate          | Purpose                                                                                       |
| ------------------ | --------------------------------------------------------------------------------------------- |
| `AiProvider`       | Configured provider (Gemini, OpenAI, DeepSeek): credentials reference, base URL, status.      |
| `AiRuntimeRoute`   | Routing rule: maps request type / language / level → provider+model. Versioned, switchable.   |
| `PromptTemplate`   | Versioned prompt template with placeholders. Multiple versions can coexist; one is active.    |
| `AiRequestLog`     | Per-call audit: provider, model, prompt template version, token in/out, latency, cost, status, error. |

## Contracts surface

- `DTOs/` — shared request/response DTOs (`AiCompletionRequest`, `AiCompletionResult`, `SpeakingTurnScoringRequest`, etc.).
- `Services/` — service interfaces consumed by other modules:
  - `IAIService` — generic completion / chat.
  - Domain-specific: `IGrammarFeedbackService`, `ISpeakingEvaluator`, `IPronunciationScorer`, `IVocabularyExampleGenerator`, ...

No integration events produced.

## Key behaviors

- Routing strategy chooses provider+model per request based on type/language/level/cost.
- Every external call writes an `AiRequestLog` row (in `ai.AiRequestLogs`) with token counts and estimated cost.
- Rate-limit / quota enforcement runs in Application **before** provider dispatch. Excess requests fail with a typed error.
- Prompt templates carry language context placeholders (`{nativeLang}`, `{targetLang}`, `{level}`, `{userTurn}`, ...). Versioning lets you A/B test prompt changes.
- All provider HTTP clients live in `AI.Infrastructure` and are not registered for DI outside the AI module.

## Notes for changes

- Adding a provider: implement the client in `AI.Infrastructure`, add an `AiProvider` row, optionally add routing rules. No change needed in other modules.
- Adding a new AI request type: define request/response DTOs + service interface in `AI.Contracts`; implement in `AI.Application`; add a prompt template version.
- Changing routing logic: keep it inside `AI.Application/Routing/`. Other modules should never know which provider answered.
