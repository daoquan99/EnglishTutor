# Documentation rules (frontend)

Frontend docs live in `EnglishTutor.Web/docs/frontend/` (if present) and mirror backend doc structure for the FE surface. Backend API docs in the repo root `docs/api/` are the **source of truth** for endpoints — don't duplicate them here.

## What FE docs cover (when present)

- `docs/frontend/architecture/` — folder structure, design system, app architecture decisions.
- `docs/frontend/routes/` — route map + auth requirements.
- `docs/frontend/workflows/` — end-to-end UI flows (e.g., vocabulary review, speaking session, dashboard composition).

## When to update

| Change                                            | Update                                             |
| ------------------------------------------------- | -------------------------------------------------- |
| New page / route                                  | `docs/frontend/routes/`                            |
| New feature module                                | `docs/frontend/architecture/` (if structural)      |
| Cross-feature UI workflow change                  | `docs/frontend/workflows/`                         |
| New shared API client behavior (refresh, retries) | `docs/frontend/architecture/`                      |
| Design-system token change                        | `docs/frontend/architecture/design-system.md`      |

If a doc file is referenced here but missing on disk, **ask** before fabricating. The folder may have been intentionally pruned.

## What to keep in code, not docs

- Type definitions — they live in `src/features/{feature}/types/` and `src/shared/types/`.
- Component prop documentation — use TypeScript types + JSDoc on exported helpers when useful. Don't replicate them in markdown.
- Query keys — `src/features/{feature}/api/query-keys.ts` is canonical.

## Backend API alignment

Endpoints, request/response DTOs, error codes are owned by backend. FE docs link to `docs/api/{module}.md` — don't restate the contract here. If the FE deviates from the backend contract for UI shaping, document the diff explicitly.

## Inline code comments

- Default no comments. Only explain non-obvious *why*.
- No "added for the X flow" comments — they rot.
- No JSDoc on internal components — TS + props names carry it.

## Forbidden

- Restating backend API contract details in FE docs.
- Adding marketing-style language to architecture docs.
- Writing docs for components that don't exist yet.
- Per-PR changelog files in `docs/` (use commit / PR messages).
