---
name: feature-implementation
description: Implements a new frontend feature (route + components + hooks + API) for a backend bounded context. Follows the feature-module shape and integrates with the centralized API client and TanStack Query.
---

# Feature Implementation

## When to invoke

- New frontend feature (a backend bounded context surfaces a new UX flow).
- Major addition to an existing feature (new page, new mutation, new realtime push).

## Inputs

- Feature slug (e.g., `vocabulary`, `speaking`).
- Backend endpoint(s) being consumed — read the matching `docs/api/{module}.md`.
- Route(s) the feature lives under (e.g., `/vocabulary`, `/speaking/[sessionId]`).

## Step-by-step

### 1. Read the contracts

- `docs/api/{module}.md` for the endpoints / request / response / error codes.
- `docs/workflows/{workflow}.md` if cross-feature.
- `EnglishTutor.API/.agents/modules/{module}.md` to understand the bounded context.

### 2. Add types

`src/features/{feature}/types/{feature}.ts`:

```ts
export interface VocabularyItemDto { ... }
export interface StudyCardResponse { ... }
```

Match backend DTO names exactly, including `Utc` suffix on timestamps. Don't mix backend DTOs with UI view models if shapes diverge.

### 3. Add Zod schemas (if forms)

`src/features/{feature}/schemas/{feature}-schemas.ts` for form validation. Mirror backend validation but adapt error messages for the UI language.

### 4. Add API functions

`src/features/{feature}/api/{feature}-api.ts`:

```ts
import { httpClient } from '@/shared/api/http-client'
import type { StudyCardResponse, ReviewRequest } from '../types/vocabulary'

export const vocabularyApi = {
  getStudyCard: (id: string) => httpClient.get<StudyCardResponse>(`/vocabulary/items/${id}/study-card`),
  review: (id: string, body: ReviewRequest) => httpClient.post<void>(`/vocabulary/items/${id}/review`, body),
}
```

### 5. Add query keys

`src/features/{feature}/api/query-keys.ts`:

```ts
export const vocabularyKeys = {
  all: ['vocabulary'] as const,
  studyCard: (id: string) => [...vocabularyKeys.all, 'studyCard', id] as const,
}
```

### 6. Add hooks

Queries + mutations as hooks in `src/features/{feature}/hooks/`:

```ts
// use-study-card.ts
export function useStudyCard(id: string) {
  return useQuery({
    queryKey: vocabularyKeys.studyCard(id),
    queryFn: () => vocabularyApi.getStudyCard(id),
  })
}

// use-review-vocabulary.ts
export function useReviewVocabulary(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: ReviewRequest) => vocabularyApi.review(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: vocabularyKeys.studyCard(id) }),
  })
}
```

### 7. Add components

`src/features/{feature}/components/` — compose shadcn primitives + Tailwind. Feature-specific only. Don't put generic UI in `shared/components/ui` from a feature task.

### 8. Wire the route

`src/app/(app)/{feature}/page.tsx`:

```tsx
import { VocabularyList } from '@/features/vocabulary/components/vocabulary-list'

export default function Page() {
  return <VocabularyList />
}
```

Keep `app/` thin — composition only.

### 9. Error & loading states

- Loading: skeleton in the feature component.
- Empty: friendly one-liner + CTA.
- Error: surface `ApiError.code` to a user-friendly message; field-level errors via react-hook-form's `setError` if a form is involved; transient errors via `sonner` toast.

### 10. Realtime (if applicable)

If the feature subscribes to notifications (e.g., session-completed, achievement), wire into `features/notifications/hooks/use-realtime-notifications.ts`. Don't open a separate hub connection.

### 11. index.ts

Export only what other features genuinely need (the route page does *not* need an export — it's imported by `app/`).

### 12. Tests

Currently no test runner is configured by default. If introducing one, scope it to the feature (Vitest / RTL). Otherwise rely on TypeScript + ESLint + manual verification.

## Commands

```bash
cd EnglishTutor.Web
pnpm dev          # turbopack with HTTPS via local certs
pnpm typecheck    # tsc --noEmit
pnpm lint
pnpm build
```

## Done when

- All four layers (types, api, hooks, components) wired.
- Route renders in `pnpm dev`.
- `pnpm typecheck` and `pnpm lint` pass.
- Loading / empty / error states implemented.
- Backend DTO field names (including `Utc` suffix) match exactly.
- No `fetch` call outside `shared/api/`.
- No business logic in `app/`.
