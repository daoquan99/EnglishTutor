# EnglishTutor.Web — .agents/

Project-specific guidance for the frontend (Next.js App Router + React 19 + TypeScript). Concise references — the source of truth is the code, `package.json`, `tsconfig.json`, and `components.json`.

Repo-wide entry point: `CLAUDE.md` at the solution root.

## Layout

```
.agents/
├── rules/      # cross-cutting rules
└── skills/     # task playbooks
```

## When to read what

| Task                                          | Read                                                              |
| --------------------------------------------- | ----------------------------------------------------------------- |
| New feature route + components                | `skills/feature-implementation.md` + `rules/frontend.md`           |
| New / changed UI primitive                    | `skills/ui-component.md` + `rules/ui-design-system.md`             |
| New backend endpoint to consume               | `skills/api-client.md` + `rules/api-client.md`                     |
| Audio / speaking UX                           | `skills/audio-speaking.md` + backend `workflows/speaking-session.md` |
| Routing structure / route groups              | `rules/frontend.md`                                                |
| Admin panel feature (CRUD, permissions)       | `skills/admin-feature.md` + `rules/frontend.md`                   |
| Realtime / SignalR notifications              | `skills/realtime-notifications.md`                                 |
| Form with validation                          | `skills/form-implementation.md` + `rules/frontend.md`              |
| Design tokens / accessibility / dark mode     | `rules/ui-design-system.md`                                        |
| Frontend docs                                 | `rules/documentation.md`                                           |

## Rules index

- `rules/frontend.md` — stack, folder layout, server/client component split, state, forbidden additions.
- `rules/api-client.md` — `shared/api/` structure, auth refresh, error normalization, query keys.
- `rules/ui-design-system.md` — component placement, shadcn, Tailwind, dark mode, accessibility.
- `rules/documentation.md` — what FE docs cover; backend `docs/api/` is the contract source of truth.

## Skills index

- `skills/feature-implementation.md` — wire a new feature end-to-end (types → api → hooks → components → route).
- `skills/ui-component.md` — pick the right placement; respect tokens; accessibility; CVA variants.
- `skills/api-client.md` — add endpoints through the centralized client; query keys; error handling.
- `skills/audio-speaking.md` — MediaRecorder, mic permissions, upload, transcript/correction UI.
- `skills/admin-feature.md` — admin panel with permission gates, data tables, CRUD dialogs.
- `skills/realtime-notifications.md` — SignalR connection, push handling, query invalidation, toasts.
- `skills/form-implementation.md` — react-hook-form + Zod schemas + server error mapping.

## Cross-references to backend

- API contracts: `docs/api/{module}.md` at repo root.
- Workflows the FE participates in: `docs/workflows/{workflow}.md` at repo root + `EnglishTutor.API/.agents/workflows/`.
- Bounded context background for each feature folder: `EnglishTutor.API/.agents/modules/{module}.md`.

Don't restate the backend contract in FE files — link.

## Conflict resolution

If `.agents/` conflicts with `CLAUDE.md` at repo root, `CLAUDE.md` wins.

If a referenced doc isn't on disk, **ask** before fabricating.
