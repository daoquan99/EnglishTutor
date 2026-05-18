# AI Module

## Responsibility
Owns AI provider integration, model routing, prompt templates, usage logging, quota, cost estimation.

## Schema
`ai`

## Tables
- `ai.AiRequestLogs` — UserId, TaskType, ModelUsed, Tokens, LatencyMs, Status
- `ai.PromptTemplates` — Name, TaskType, Description, IsActive
- `ai.PromptVersions` — TemplateId, VersionNumber, SystemPrompt, UserPromptTemplate, IsActive
- `ai.ModelRoutingRules` — TaskType, PreferredModel, FallbackModel, MaxTokens, Temperature
- `ai.AiUsageCounters` — Per-user/model/task usage tracking
- `ai.AiCostEstimations` — Cost per model per token

## Model Routing
| Task | Model |
|------|-------|
| Short sentence correction | Gemma |
| Daily exercise generation | Gemma |
| Vocabulary examples | Gemma |
| Long writing correction | Gemini Flash |
| Deep grammar explanation | Gemini Flash |
| Assessment grading | Gemini Flash+ |
| Realtime voice | Gemini Live |
| TTS audio | Gemini TTS |

## Contract Interfaces (AI.Contracts)
- `IEnglishCorrectionService` — Sentence correction
- `IAiExerciseGenerator` — Exercise generation
- `IAiLessonGenerator` — Lesson generation
- `IAssessmentGradingService` — Assessment grading with rubrics
- `IAudioGenerationService` — TTS audio generation

## CRITICAL RULE
All Gemini/Gemma client code MUST exist only in `AI.Infrastructure`. No other module may call AI providers directly.
