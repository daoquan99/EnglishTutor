# ADR-0001: Modular Monolith Architecture

## Status
Accepted

## Context
We need an architecture for an AI-powered English Tutor SaaS system with 13+ bounded contexts. Options considered:
1. **Traditional Monolith** — fast to build, hard to maintain at scale, no module isolation
2. **Microservices** — full isolation, but extremely complex for a team starting out (networking, deployment, debugging)
3. **Modular Monolith** — module isolation within a single deployment unit

## Decision
Adopt **Modular Monolith** with Clean Architecture per module and DDD tactical patterns.

Each module (Auth, Users, Vocabulary, Speaking, etc.) is isolated at the code level:
- Own DbContext and database schema
- Own domain, application, infrastructure, presentation layers
- Communication via Contracts (sync reads) and Integration Events (state changes)
- Single deployable unit (API Host + Worker Host)

## Consequences
**Benefits:**
- Module isolation without distributed systems complexity
- Single deployment and debugging experience
- Can extract modules to microservices later if needed
- Simpler CI/CD pipeline

**Trade-offs:**
- Must enforce boundaries via architecture tests (no compiler enforcement like separate repos)
- All modules share the same runtime process — one module's memory leak affects all
- Cannot scale individual modules independently (yet)
