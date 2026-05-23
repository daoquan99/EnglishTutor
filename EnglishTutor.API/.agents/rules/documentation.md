# Documentation rules

Docs are part of the Definition of Done. Update them in the same task as the code change.

## Locations

| Topic                              | Location                              |
| ---------------------------------- | ------------------------------------- |
| HTTP API surface per module        | `docs/api/{module}.md`                |
| Cross-module business workflows    | `docs/workflows/{workflow}.md`        |
| Integration events catalog         | `docs/events/integration-events.md`   |
| Outbox/Inbox reference             | `docs/events/outbox-inbox.md`         |
| Architecture Decision Records      | `docs/adr/{NNNN}-{slug}.md`           |

Agent-only specifications (not user-facing) live in `EnglishTutor.API/.agents/` (this directory).

## When to update

| Change                                            | Update                                      |
| ------------------------------------------------- | ------------------------------------------- |
| New / changed endpoint                            | `docs/api/{module}.md`                      |
| Request / response DTO change                     | `docs/api/{module}.md`                      |
| Validation rule change                            | `docs/api/{module}.md`                      |
| Auth / permission change for an endpoint          | `docs/api/{module}.md`                      |
| New integration event                             | `docs/events/integration-events.md`         |
| Integration event payload change                  | `docs/events/integration-events.md` + bump  |
| Business workflow change                          | `docs/workflows/{workflow}.md`              |
| Read model / projection change                    | matching workflow doc + module doc          |
| Module communication change (new Contract Reader) | architecture / workflow docs                |
| Architecture-level decision                       | new ADR in `docs/adr/`                      |

If the doc file is missing on disk but referenced here, **ask before fabricating** — it may be deleted intentionally. Don't make up sections to fill space.

## API doc template

```markdown
# {Module} API

## {Endpoint Title}

- **Endpoint:** `METHOD /path/{param}`
- **Purpose:** one sentence.
- **Auth:** permission code(s) required, or `Anonymous`.
- **Request body:**
- **Response body:** 200 / 201 / 204 + error codes.
- **Validation rules:** bulleted.
- **Error codes:** stable `Module.ErrorName` strings from the Application error catalog.
- **Application flow:** what the handler does (3–6 bullets).
- **Related modules:** Contract Readers used, integration events produced.
- **Integration events produced:** list with link to event doc.
- **Integration events consumed:** if applicable.
- **Read models / projections updated:** if applicable.
```

## Workflow doc template

```markdown
# {Workflow Name}

## Overview
One paragraph.

## Main flow
Sequence diagram or ordered list of cross-module steps.

## Detailed steps
Bullets, one per module involved.

## Modules involved
List with link to each module's `.agents/modules/{module}.md`.

## Contracts used
List of Contract Readers (interface name + owning module).

## Events published / consumed
Link to event doc entries.

## Read models / projections updated
List.

## Failure / retry behavior
What happens when AI fails, an event handler throws, etc.
```

## Event doc template

Each event entry in `docs/events/integration-events.md`:

```markdown
### {EventName}IntegrationEvent

- **Producer:** {Module}
- **Consumers:** {Module}, {Module}, ...
- **Payload:**
- **Published when:** ...
- **Side effects:** ...
- **Idempotency:** key used by consumers (typically `(EventId, ConsumerName)`).
- **Related workflows:** link(s).
- **Version:** v1 (bump on payload change).
```

## Forbidden

- Adding or changing an API endpoint without updating its doc.
- Renaming integration events without updating the event doc.
- Workflow changes that don't update the workflow doc.
- Writing docs for code that doesn't exist yet ("planned" features).
