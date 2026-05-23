# AGENTS.md

## Project Type

This is the frontend repository for EnglishTutor, an AI-powered English learning SaaS application.

The frontend stack is:

- Next.js App Router
- React
- TypeScript
- Tailwind CSS
- shadcn/ui
- TanStack Query
- React Hook Form
- Zod
- Recharts
- lucide-react

Do not introduce another UI framework or design system unless explicitly requested.

---

## Product Context

EnglishTutor helps users learn a target language using:

- Study plans
- Vocabulary flashcards
- Example sentence pronunciation practice
- Exercises
- Speaking practice
- Mistake review
- Assessments and level-up tests
- Progress dashboards
- Notifications
- Admin reports

The backend is a production-grade .NET modular monolith. The frontend should mirror backend business capabilities through feature-based modules.

---

## Core Frontend Architecture

Use this structure:

```text
src
├── app
│   ├── (public)
│   ├── (auth)
│   ├── (app)
│   ├── (admin)
│   ├── layout.tsx
│   └── globals.css
│
├── features
│   ├── auth
│   ├── users
│   ├── study-plans
│   ├── learning-content
│   ├── vocabulary
│   ├── exercises
│   ├── speaking
│   ├── mistakes
│   ├── assessments
│   ├── progress
│   ├── notifications
│   └── admin-reports
│
└── shared
    ├── api
    ├── components
    │   └── ui
    ├── hooks
    ├── lib
    ├── types
    └── utils
```

Rules:

- `src/app` is for routing, layouts, loading states, error boundaries, and page composition.
- `src/features` is for business features.
- `src/shared` is for reusable cross-feature utilities and UI primitives.
- Do not put feature-specific business logic into `shared`.
- Do not put large business components directly in `src/app`.
- App routes should compose feature components.

---

## Route Groups

Use Next.js App Router route groups:

```text
src/app/(public)
src/app/(auth)
src/app/(app)
src/app/(admin)
```

Suggested routes:

```text
(public)
- /
- /pricing
- /about

(auth)
- /login
- /register
- /forgot-password

(app)
- /dashboard
- /study-plan
- /vocabulary
- /vocabulary/[id]
- /exercises
- /exercises/[id]
- /speaking
- /speaking/[sessionId]
- /mistakes
- /assessments
- /progress
- /notifications
- /settings

(admin)
- /admin
- /admin/users
- /admin/reports
- /admin/ai-usage
- /admin/content
```

---

## Feature Module Structure

Each feature should follow this structure when needed:

```text
src/features/{feature-name}
├── api
├── components
├── hooks
├── schemas
├── types
├── utils
└── index.ts
```

Rules:

- Feature API clients stay inside `features/{feature}/api`.
- Feature-specific components stay inside `features/{feature}/components`.
- Feature-specific types stay inside `features/{feature}/types`.
- Feature-specific validation schemas stay inside `features/{feature}/schemas`.
- Shared UI primitives stay in `shared/components/ui`.
- Shared layout/application components stay in `shared/components`.

---

## shadcn/ui Rules

shadcn/ui is the core component system.

Use this location for generated UI primitives:

```text
src/shared/components/ui
```

Use this location for shadcn utility helpers:

```text
src/shared/lib/utils.ts
```

`components.json` should use aliases similar to:

```json
{
  "aliases": {
    "components": "@/shared/components",
    "utils": "@/shared/lib/utils",
    "ui": "@/shared/components/ui",
    "lib": "@/shared/lib",
    "hooks": "@/shared/hooks"
  }
}
```

Rules:

- Do not install another heavy UI framework such as MUI, Ant Design, Chakra, Mantine, or daisyUI unless explicitly requested.
- Prefer shadcn/ui primitives plus custom Tailwind styling.
- Do not modify generated shadcn/ui components heavily for one-off feature needs.
- If a shadcn/ui primitive needs project-wide style changes, keep changes generic and reusable.
- Feature-specific visual composition should be done in feature components, not inside `shared/components/ui`.

---

## Tailwind CSS Rules

Use Tailwind CSS as the main styling system.

Rules:

- Use Tailwind utility classes for layout and component styling.
- Keep global CSS minimal.
- `src/app/globals.css` should contain Tailwind imports, CSS variables, theme tokens, base body styles, and design system foundations only.
- Do not create large global CSS files for feature-specific styling.
- Avoid inline styles unless dynamic values are impossible with Tailwind.
- Use consistent spacing, radius, typography, and color tokens.
- Prefer responsive utility classes over custom media queries.

---

## Design System Direction

The UI should feel:

- Clean
- Modern
- Calm
- Friendly
- Professional
- SaaS-ready
- Learning-focused

Avoid:

- Overly playful children-style UI
- Too many gradients
- Too many colors
- Generic admin-template look
- Inconsistent shadows/radii
- Overly complex animations

The product should feel suitable for adult learners and developers.

---

## TypeScript Rules

- Use strict TypeScript.
- Avoid `any`.
- Use explicit domain types for API responses.
- Use Zod schemas for form validation and complex request validation.
- Keep DTO types aligned with backend API documentation.
- Do not duplicate types unnecessarily.
- Prefer discriminated unions for status-heavy UI states.
- Do not store backend DTOs and UI view models as the same type if the UI shape is different.

---

## API Client Rules

Use a centralized API client in:

```text
src/shared/api
```

Recommended structure:

```text
src/shared/api
├── http-client.ts
├── api-error.ts
├── auth-token-store.ts
└── query-client.ts
```

Rules:

- Feature API functions should call the shared HTTP client.
- Do not call `fetch` or `axios` directly from page components.
- Do not scatter base URLs across the codebase.
- Use environment variables for API base URL.
- Handle 401/403 consistently.
- Normalize API errors into a shared `ApiError` shape.
- Do not silently swallow API errors.
- Do not put business-specific API calls in `shared/api`.

---

## Server State Rules

Use TanStack Query for server state.

Rules:

- Use queries for GET/read operations.
- Use mutations for create/update/delete/commands.
- Keep query keys consistent and colocated with feature API/hook files.
- Do not duplicate server state into global client state unnecessarily.
- Use optimistic updates only when the UX benefit is clear.
- Invalidate affected queries after mutations.
- Keep loading/error/empty states explicit.

---

## Forms Rules

Use:

- React Hook Form
- Zod
- shadcn/ui form components

Rules:

- Define schemas in `features/{feature}/schemas`.
- Keep form submit logic in feature hooks or feature components.
- Show field-level validation errors.
- Keep server validation errors visible.
- Do not put form schemas in page components if reused.

---

## Audio and Speaking UI Rules

Speaking and pronunciation features may use:

- MediaRecorder API
- Web Audio API
- browser microphone permissions

Rules:

- Always show microphone permission states.
- Always show recording/loading/error states.
- Keep audio recording logic in feature hooks.
- Do not put raw audio handling logic directly in page components.
- Provide retry behavior for failed uploads/analysis.
- Keep accessibility in mind for audio controls.

---

## Documentation Rules

Whenever a route, page, API client, feature workflow, or shared UI pattern is added or changed, update related documentation in the same task.

Documentation locations:

```text
docs/frontend/architecture
docs/frontend/routes
docs/frontend/features
docs/frontend/workflows
docs/frontend/design-system
```

Rules:

- Do not add or change a route without updating route docs.
- Do not add or change a feature workflow without updating workflow docs.
- Do not change API request/response usage without updating feature docs.
- Do not add shared UI patterns without updating design-system docs.
- Do not change folder structure without updating architecture docs.
- Documentation updates are part of the Definition of Done.

---

## Testing Rules

Before completing a task, run:

```bash
pnpm lint
pnpm typecheck
pnpm test
```

If commands do not exist or cannot run, explain why.

---

## Before Changing Code

Before making changes:

1. Inspect existing patterns.
2. Reuse existing conventions.
3. Explain the files you plan to modify.
4. Keep changes minimal and focused.
5. Do not change architecture decisions without asking.

---

## Definition of Done

A task is not complete unless all applicable items are done:

- Code compiles.
- TypeScript passes.
- Lint passes.
- Tests pass or the agent explains why they cannot run.
- New/updated routes are documented.
- New/updated feature workflows are documented.
- New/updated API client usage is documented.
- New/updated shared UI patterns are documented.
- Loading, error, empty, and success states are handled.
- Accessibility basics are respected.
- No unrelated UI library is introduced.
- No architecture rule is violated.

When finishing a task, provide a summary with:

```text
- Changed files
- Routes changed
- Components changed
- API clients changed
- State/query changes
- Docs updated
- Tests run
- Known limitations or follow-up tasks
```

---

## Forbidden

- Do not rewrite the whole project.
- Do not change architecture without asking.
- Do not add MUI, Ant Design, Chakra, Mantine, daisyUI, or another UI library unless explicitly requested.
- Do not use Redux unless explicitly requested.
- Do not put feature-specific business logic in `shared`.
- Do not put large interactive business components directly in `src/app`.
- Do not call backend APIs directly from random components.
- Do not hard-code API base URLs.
- Do not use `any` as a shortcut.
- Do not mark entire apps/layouts as client components unnecessarily.
- Do not create large global CSS for feature-specific styling.
- Do not add routes without updating documentation.
