# Frontend rules

Next.js App Router + React 19 + TypeScript strict.

## Stack (fixed unless explicitly approved)

- Next.js (App Router, turbopack dev)
- React 19
- TypeScript strict
- Tailwind v4
- shadcn/ui (Base UI primitives under the hood)
- TanStack Query (server state)
- Zustand (client state)
- react-hook-form + Zod (forms)
- Recharts (charts)
- @microsoft/signalr (realtime notifications)
- lucide-react (icons)
- sonner (toasts)

Do not add another UI framework (MUI / AntD / Chakra / Mantine / daisyUI), state manager, or form library.

## Folder layout

```
src/
├── app/                # routes only — no business logic
│   ├── (public)/
│   ├── (auth)/
│   ├── (app)/
│   ├── (admin)/
│   ├── layout.tsx
│   └── globals.css
├── features/           # business features mirroring backend bounded contexts
│   ├── auth/
│   ├── users/
│   ├── study-plans/
│   ├── learning-content/
│   ├── vocabulary/
│   ├── exercises/
│   ├── speaking/
│   ├── mistakes/
│   ├── assessments/
│   ├── progress/
│   ├── notifications/
│   └── admin-reports/
└── shared/             # cross-feature primitives
    ├── api/
    ├── components/
    │   └── ui/         # shadcn primitives
    ├── hooks/
    ├── lib/
    ├── types/
    └── utils/
```

Rules:
- `src/app` is for routing, layouts, loading/error boundaries, and page composition only. No business components defined inline.
- `src/features/{feature}` is for everything domain-specific.
- `src/shared` is for reusable cross-feature primitives only. Don't put feature-specific business logic here.

## Feature module shape

```
src/features/{feature}/
├── api/
├── components/
├── hooks/
├── schemas/        # Zod schemas
├── types/
├── utils/
└── index.ts
```

- Feature API functions stay in `features/{feature}/api`.
- Feature-specific components stay in `features/{feature}/components`.
- Feature-specific types stay in `features/{feature}/types`.
- Don't expose feature internals from `index.ts` beyond what other features genuinely need.

## TypeScript

- Strict mode. No `any`. Avoid `unknown` unless followed by a narrowing guard.
- Explicit types for API responses. Use Zod to parse at the network boundary.
- Use discriminated unions for status-heavy UI states (`'idle' | 'loading' | 'success' | 'error'`).
- Keep API DTOs and UI view models separate when the shape diverges.

## Tailwind

- Use utility classes. Keep `globals.css` minimal (Tailwind imports + CSS variables + base body styles).
- No feature-specific CSS files. No inline styles unless a dynamic value is impossible in Tailwind.
- Use the design tokens (spacing / radius / typography / color) consistently. See `ui-design-system.md`.

## Server vs client components

- Default to Server Components.
- Mark `'use client'` only when needed (event handlers, hooks, state, browser APIs).
- Keep `'use client'` boundary as close to the leaf as possible — don't make whole feature pages client components.
- Server Components fetch via the server-side API client; Client Components fetch via TanStack Query hooks.

## State

- Server state → TanStack Query. Don't shadow it in Zustand or Context.
- Ephemeral UI state → `useState`/`useReducer`.
- Cross-component client state (theme, sidebar collapsed, auth token) → Zustand store in `shared/lib/stores/` or `features/{feature}/hooks/`.

## Forbidden

- Another UI framework or design system.
- Calling fetch directly in components — go through the API client (`api-client.md`).
- Putting business logic into `app/` or `shared/`.
- Hard-coded backend URLs / colors / spacings outside the design system.
- `console.log` left in committed code.
- Adding a route group beyond the four listed without coordination.
