# Phase X: AI Provider Registry & Runtime Routing

## Goal

Add an admin-configurable AI provider registry and runtime route resolver so text generation, TTS, STT, live audio, grading/scoring, lesson generation, and exercise generation can switch provider/model without changing caller modules.

## Scope

- AI module owns all provider registry data and routing logic.
- Other modules continue using `AI.Contracts`.
- Provider clients stay inside `AI.Infrastructure`.
- Raw API keys are not stored in database rows; provider rows keep a secret/config key name only.
- No RabbitMQ/Kafka/external event bus is introduced.

## Tasks

- [x] Add provider registry domain model.
- [x] Add provider model capability domain model.
- [x] Add runtime route domain model.
- [x] Add repository abstractions and EF persistence.
- [x] Seed default providers, provider models, and runtime routes.
- [x] Add runtime route resolver with fallback to legacy routing rules.
- [x] Route existing AI requests through runtime provider/model metadata.
- [x] Add admin endpoints for providers, provider models, routes, and route resolution.
- [x] Add AI docs and workflow docs.
- [x] Add focused domain/router unit tests.

## Deferred

- Real external SDK integrations for each provider.
- Secret-store integration beyond storing secret/config key names.
- Provider health checks.
- Provider-specific retry/circuit-breaker policy.
- Async long-running AI jobs through Worker and Outbox/Inbox.
- Admin UI.
