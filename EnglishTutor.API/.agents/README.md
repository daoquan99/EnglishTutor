# EnglishTutor.API — .agents/

Project-specific guidance for agents (Claude Code, other AI assistants, human contributors). Concise references — the source of truth is the code and the docs in `docs/`.

Repo-wide entry point: `CLAUDE.md` at the solution root.

## Layout

```
.agents/
├── rules/        # cross-cutting rules (one per concern)
├── modules/      # one file per bounded context (13 modules)
├── workflows/    # cross-module business flows
└── skills/       # task playbooks (architecture-guardian, module-implementer, ...)
```

## When to read what

| Task                                                  | Read                                                                          |
| ----------------------------------------------------- | ----------------------------------------------------------------------------- |
| Small local change inside a single module             | `modules/{module}.md` + relevant `rules/*` for the concern you're touching     |
| New endpoint / command in a module                    | `skills/module-implementer.md` + `modules/{module}.md` + `rules/coding.md`     |
| Cross-module workflow change                          | `workflows/{workflow}.md` + each affected `modules/{module}.md`                |
| New integration event or consumer                     | `skills/integration-events-agent.md` + `rules/events.md`                       |
| Architecture review / new boundary                    | `rules/architecture.md` + `skills/architecture-guardian.md`                    |
| Tests                                                 | `rules/testing.md` + `skills/test-writer.md`                                   |
| Adding / changing API docs                            | `rules/documentation.md` + `skills/documentation-agent.md`                     |
| Database migration                                    | `skills/migration.md` + `rules/database.md`                                    |
| Cross-module read (Contract Reader)                   | `skills/contract-reader.md` + `rules/architecture.md`                          |
| Seed / reference data                                 | `skills/seeder.md`                                                             |
| AI integration                                        | `rules/ai.md` + `modules/ai.md`                                                |
| Permission / auth change                              | `rules/authorization.md` + `modules/auth.md`                                   |
| Multi-language / timezone                             | `rules/multi-language.md`                                                      |
| Package add / version bump                            | `rules/packages.md`                                                            |

## Rules index

- `rules/architecture.md` — modular monolith boundaries, cross-module comms, hosts.
- `rules/coding.md` — language, style, what not to add, comments, time.
- `rules/testing.md` — test project layout, conventions, commands.
- `rules/database.md` — schema per module, EF rules, migrations.
- `rules/events.md` — Domain vs Integration events, Outbox/Inbox, idempotency.
- `rules/domain-modeling.md` — aggregate folder layout, soft-delete, audit, UTC.
- `rules/authorization.md` — permission-based access, wildcard, no role gating.
- `rules/ai.md` — AI module isolation, prompt versioning, cost logging.
- `rules/multi-language.md` — four language codes, scoping, timezone.
- `rules/error-handling.md` — Domain exceptions vs Application `Result`/`Error`.
- `rules/documentation.md` — doc obligations and templates.
- `rules/packages.md` — Central Package Management, no per-csproj versions.
- `rules/hosts.md` — Api vs Worker vs AppHost responsibilities.

## Modules index

13 bounded contexts: `auth`, `users`, `studyplans`, `learningcontent`, `vocabulary`, `exercises`, `speaking`, `ai`, `mistakes`, `assessments`, `progress`, `notifications`, `adminreports`.

## Workflows index

- `workflows/speaking-session.md`
- `workflows/vocabulary-flashcard-review.md`
- `workflows/exercise-completion.md`
- `workflows/level-up-assessment.md`
- `workflows/study-reminder.md`
- `workflows/progress-dashboard.md`

## Skills index

Each skill file is a playbook for a recurring task type. They include step-by-step actions, conventions, commands, and a "done when" checklist.

- `skills/architecture-guardian.md` — boundary / layering / package verification.
- `skills/module-implementer.md` — new endpoint / command / aggregate in a module.
- `skills/test-writer.md` — domain / handler / integration / architecture tests.
- `skills/documentation-agent.md` — API / event / workflow / ADR docs.
- `skills/integration-events-agent.md` — Outbox / Inbox / Worker wiring.
- `skills/migration.md` — EF Core migration generation, naming, verification.
- `skills/contract-reader.md` — cross-module read-only data via Contracts project.
- `skills/seeder.md` — module seed data (reference data, dev fixtures, IModuleSeeder).

## Conflict resolution

If `.agents/` content conflicts with `CLAUDE.md`, `CLAUDE.md` wins and the discrepancy should be reported.

If `.agents/` conflicts with the code, the code wins for current behavior — but flag the gap so the docs catch up.

If a referenced file (e.g. a workflow doc in `docs/workflows/`) doesn't exist on disk, **ask** before fabricating one.
