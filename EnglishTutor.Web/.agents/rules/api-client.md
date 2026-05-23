# API client rules

## Layout

```
src/shared/api/
├── http-client.ts        # fetch wrapper + auth header + JSON + error normalization
├── api-error.ts          # ApiError class with status / code / message / fieldErrors
├── auth-token-store.ts   # access/refresh token persistence + refresh flow
└── query-client.ts       # TanStack Query client config (retry, staleTime, etc.)
```

Per-feature API functions live in `src/features/{feature}/api/{feature}-api.ts` and call into `http-client`. Query keys live in `src/features/{feature}/api/query-keys.ts`.

## http-client

- Wraps `fetch` (or undici). Single base URL from env (`NEXT_PUBLIC_API_BASE_URL`).
- Attaches `Authorization: Bearer <token>` from `auth-token-store`.
- Sets `Accept: application/json`. Serializes JSON requests.
- On 401: attempt refresh via `auth-token-store`; retry once; on continued 401, sign out.
- Parses ProblemDetails error responses into `ApiError` (status, code, message, validationErrors).
- Surfaces network errors as `ApiError` with a synthetic code.

## auth-token-store

- Stores access + refresh tokens. Choose storage (httpOnly cookie via server route preferred, `localStorage` acceptable for non-sensitive setups).
- Provides `getAccessToken()`, `setTokens(...)`, `clear()`, `refresh()`.
- `refresh()` is single-flight — concurrent callers share the same promise.
- On refresh failure: clear tokens and bubble a signed-out state.

## query-client

- Configure: `staleTime`, `gcTime`, `retry` (skip on 4xx), `refetchOnWindowFocus` policy.
- One shared client for the whole app (`QueryClientProvider` in the root layout).

## Per-feature API

```ts
// features/vocabulary/api/vocabulary-api.ts
import { httpClient } from '@/shared/api/http-client'
import { StudyCardResponse } from '../types/vocabulary'

export const vocabularyApi = {
  getStudyCard: (id: string) =>
    httpClient.get<StudyCardResponse>(`/vocabulary/items/${id}/study-card`),
  markMastered: (id: string) =>
    httpClient.post<void>(`/vocabulary/items/${id}/mark-mastered`),
}
```

Query keys:

```ts
// features/vocabulary/api/query-keys.ts
export const vocabularyKeys = {
  all: ['vocabulary'] as const,
  studyCard: (id: string) => [...vocabularyKeys.all, 'studyCard', id] as const,
  today: ['vocabulary', 'today'] as const,
}
```

Hooks:

```ts
// features/vocabulary/hooks/use-study-card.ts
export function useStudyCard(id: string) {
  return useQuery({
    queryKey: vocabularyKeys.studyCard(id),
    queryFn: () => vocabularyApi.getStudyCard(id),
  })
}
```

## Backend DTO alignment

- Property names match the backend exactly, including the `Utc` suffix on timestamps.
- Timestamps arrive as ISO-8601 UTC strings. Convert to `Date` only when formatting for display, using the user's display timezone.
- Don't store the user's timezone offset in state. Read display timezone from the browser or the user's profile.

## ProblemDetails error shape

Backend returns:

```json
{
  "type": "https://...",
  "title": "...",
  "status": 400,
  "code": "Vocabulary.CardNotFound",
  "detail": "...",
  "errors": { "field": ["message"] }
}
```

`ApiError` exposes `.code`, `.message`, `.fieldErrors`. UI displays per-field errors via react-hook-form's `setError`, and surfaces general errors via toast (`sonner`).

## Forbidden

- Calling `fetch` directly in feature components.
- Hard-coded URLs in components.
- Reading the token from `localStorage` in components (go through `auth-token-store`).
- Putting query keys inline at the call site — keep them in `query-keys.ts` per feature.
- Returning untyped `any` from API functions.
- Different feature API clients with their own fetch wrapper.
