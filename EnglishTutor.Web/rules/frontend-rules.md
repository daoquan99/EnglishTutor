# Frontend Rules

## Stack

Use:

```text
Next.js App Router
React
TypeScript
Tailwind CSS
shadcn/ui
TanStack Query
React Hook Form
Zod
Recharts
lucide-react
```

Do not add another UI framework unless explicitly requested.

## Architecture

Frontend architecture is feature-based.

```text
src/app      = routing and page composition
src/features = business features
src/shared   = reusable cross-feature utilities/components
```

## Feature Boundaries

Each feature owns its own:

```text
api
components
hooks
schemas
types
utils
```

Rules:

- Do not put feature logic into `shared`.
- Do not import deeply from another feature unless the file is explicitly exported.
- Prefer public exports through `index.ts`.
- If two features need the same primitive, move it to `shared` only if truly generic.
