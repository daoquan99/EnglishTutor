# ADR-0002: Module-Owned DbContext and Schema

## Status
Accepted

## Context
In a modular monolith, modules need data isolation. Options:
1. **Shared DbContext** — simplest, but modules can accidentally query each other's tables
2. **DbContext per module, shared schema** — some isolation, but naming collisions possible
3. **DbContext per module, schema per module** — full isolation

## Decision
Each module owns its own **DbContext** and **PostgreSQL schema**.

- Auth → `auth` schema, `AuthDbContext`
- Users → `users` schema, `UsersDbContext`
- Vocabulary → `vocabulary` schema, `VocabularyDbContext`
- etc.

Cross-module references use IDs only (no FK navigation properties across modules).

## Consequences
**Benefits:**
- No accidental cross-module queries at the EF level
- Clean migration streams per module
- Easy to identify which module owns which data
- Prepares for future microservice extraction

**Trade-offs:**
- Cannot use EF navigation properties across modules
- Must duplicate some data via Read Models for aggregate screens
- More DbContext registrations in DI
