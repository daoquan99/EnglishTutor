# AI module rules

## Single owner

All AI-provider interaction is owned by the `AI` module. Other modules call into AI via `AI.Contracts` only.

The AI module owns:

- Gemini / Gemma / OpenAI / DeepSeek clients
- Model routing (which provider/model for which request type)
- Prompt templates and versioning
- AI request logs and cost / token counters
- Rate limits and quotas
- Cost estimation

## Where AI provider clients live

Clients are defined **exclusively** in `AI.Infrastructure`. No HTTP call to a provider may originate from any other module.

## How other modules use AI

- Reference `EnglishTutor.Modules.AI.Contracts`.
- Inject `IAIService` (or domain-specific contract like `IGrammarFeedbackService`, `ISpeakingEvaluator`, ...) from AI Contracts.
- Pass language context on every call: native language code, target language code, explanation language code, user proficiency level (e.g., `Native: "vi"`, `Target: "en"`, `Level: "B2"`).
- Receive DTOs from AI Contracts. Never receive `IQueryable` or provider-specific SDK types.

## What lives in AI

- Prompt templates → `AI.Application/Prompts/` (versioned).
- Routing strategy → `AI.Application/Routing/`.
- Cost estimation → `AI.Application/Costs/` + `AI.Infrastructure/Costs/`.
- Audit log of all AI calls → `ai.AiRequestLogs` table.
- Rate-limit / quota enforcement → `AI.Application` (called before provider dispatch).

## Forbidden

- HTTP calls to Gemini/OpenAI/DeepSeek/any AI provider from any module other than `AI.Infrastructure`.
- Prompt templates duplicated inside `Speaking`, `Vocabulary`, `Exercises`, `Assessments`, etc.
- Duplicated model-routing logic in other modules.
- Other modules taking dependencies on provider SDK packages directly.
- Sending AI requests without language + level context in the payload.
- Skipping the AI request log / cost tracker.
