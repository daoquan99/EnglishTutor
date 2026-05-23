---
name: documentation-agent
description: Updates docs in lockstep with code changes — API docs, workflow docs, event docs, ADRs. Run as part of the same PR as the code change, not after.
---

# Documentation Agent

## When to invoke

In the **same task** as any of:

- New/changed HTTP endpoint.
- Request / response DTO change.
- Validation rule change.
- Auth / permission change for an endpoint.
- New integration event.
- Integration event payload change.
- Business workflow change.
- Read model / projection change.
- Architecture-level decision.

## Where docs live

| Topic                       | File                                     |
| --------------------------- | ---------------------------------------- |
| API per module              | `docs/api/{module}.md`                   |
| Workflow                    | `docs/workflows/{workflow}.md`           |
| Integration events catalog  | `docs/events/integration-events.md`      |
| Outbox/Inbox reference      | `docs/events/outbox-inbox.md`            |
| Architecture decisions      | `docs/adr/{NNNN}-{slug}.md`              |

## Templates

See `../rules/documentation.md` for the API / workflow / event templates.

## How to update

### API change

1. Open `docs/api/{module}.md` (create if missing).
2. Add/update the endpoint section using the API template.
3. Include: endpoint, purpose, auth, request body, response body (success + error codes), validation rules, error codes (matching the Application error catalog), application flow, related modules, integration events produced/consumed, read models updated.
4. Cross-link error codes to the catalog file.

### Integration event change

1. Open `docs/events/integration-events.md`.
2. New event: add a new section using the event template.
3. Payload change: bump the event version (e.g., `v1 → v2`), document the diff. If the change is breaking, note the migration window for consumers.
4. List producer + consumer modules; link to workflow docs.

### Workflow change

1. Open `docs/workflows/{workflow}.md` (create if missing).
2. Update flow diagram, detailed steps, events list, read models list, failure behavior.
3. Cross-link the affected `docs/api/...` and `docs/events/...` entries.

### Architecture decision

1. Number the ADR sequentially (look at existing entries).
2. File: `docs/adr/{NNNN}-{slug}.md`. Lowercase, kebab-case slug.
3. Sections: Status, Context, Decision, Consequences, Alternatives Considered.

## Rules

- If a doc file is missing on disk but should exist, **ask** before creating. The user may have deleted it intentionally.
- Don't write docs for code that doesn't exist yet — no aspirational sections.
- Match terminology to the code: aggregate names, event names, permission codes must be exact.
- Timestamps are UTC. Property names in docs include `Utc` suffix where the code does.
- Don't duplicate content across files — link instead.

## Done when

- Every public-surface change (API/event/workflow/contract) has a matching doc update in the same PR.
- Error code listed in docs matches the Application error catalog string exactly.
- Event versions consistent across event doc, producer code, consumer code.
