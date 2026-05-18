# ADR-0003: Cross-Module Communication Strategy

## Status
Accepted

## Context
Modules need to communicate. Three patterns are available, each for different use cases.

## Decision

### 1. Contract Readers (synchronous, immediate)
**When:** A module needs another module's data during a request.
**Example:** Speaking module reads user language settings before creating a session.
**Pattern:** Interface in Contracts project, implementation in Infrastructure.

### 2. Integration Events (asynchronous, state changes)
**When:** A business event in one module should trigger side effects in others.
**Example:** `SpeakingSessionCompletedIntegrationEvent` triggers Progress update.
**Pattern:** Outbox → Worker → Inbox pipeline.

### 3. Read Models / Projections (pre-computed, aggregate screens)
**When:** A UI screen needs data from multiple modules.
**Example:** Dashboard snapshot combines data from Progress, Speaking, Vocabulary.
**Pattern:** The module that needs the view owns and updates it via events.

## Consequences
**Benefits:**
- Clear rules for when to use which pattern
- No hidden coupling via shared database queries
- Read Models eliminate runtime cross-module joins

**Trade-offs:**
- Read Models introduce data duplication
- Contract Readers create compile-time coupling (acceptable, controlled)
- Integration Events introduce eventual consistency
