---
name: api-client
description: Adds or modifies the FE → BE HTTP integration — endpoints, auth refresh, error handling, query keys, TanStack Query hooks. Keeps everything routed through the centralized client.
---

# API Client

## When to invoke

- New backend endpoint needs to be consumed.
- Changing auth token handling / refresh flow.
- Adding a global API behavior (correlation ID header, retry policy).
- Adjusting error normalization or ProblemDetails parsing.

## Layout

```
src/shared/api/
├── http-client.ts        # fetch wrapper
├── api-error.ts          # ApiError + helpers
├── auth-token-store.ts   # tokens + refresh
└── query-client.ts       # TanStack Query config
```

Per-feature API calls live in `src/features/{feature}/api/` with query keys, never in `shared/api/`.

## Adding a new endpoint

### 1. Add the type

`src/features/{feature}/types/{feature}.ts`:

```ts
export interface ReviewRequest { grade: number }
export interface ReviewResultResponse { nextReviewAtUtc: string; ... }
```

Match backend DTO field names verbatim, including the `Utc` suffix on timestamps.

### 2. Add the API function

`src/features/{feature}/api/{feature}-api.ts`:

```ts
export const vocabularyApi = {
  review: (id: string, body: ReviewRequest) =>
    httpClient.post<ReviewResultResponse>(`/vocabulary/items/${id}/review`, body),
}
```

### 3. Add query keys

`src/features/{feature}/api/query-keys.ts`:

```ts
export const vocabularyKeys = {
  all: ['vocabulary'] as const,
  list: () => [...vocabularyKeys.all, 'list'] as const,
  detail: (id: string) => [...vocabularyKeys.all, 'detail', id] as const,
}
```

### 4. Add hook(s)

`src/features/{feature}/hooks/use-*.ts`:

```ts
export function useReviewVocabulary(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: ReviewRequest) => vocabularyApi.review(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: vocabularyKeys.detail(id) }),
  })
}
```

## Auth handling

- Tokens persisted via `auth-token-store`. Don't read `localStorage` from components.
- `http-client` attaches `Authorization: Bearer <token>` automatically.
- On 401:
  1. Single-flight `auth-token-store.refresh()`.
  2. Retry the original request once.
  3. If still 401: clear tokens, route to `/login`.
- Concurrent 401s during a single refresh share the same in-flight refresh promise.

## Error normalization

Backend returns ProblemDetails (`code`, `detail`, `errors`). `http-client` parses into `ApiError`:

```ts
class ApiError extends Error {
  status: number
  code?: string                 // e.g., "Vocabulary.CardNotFound"
  fieldErrors?: Record<string, string[]>
}
```

In components:
- Field errors → react-hook-form `setError(field, { message })`.
- General code-based errors → map to user-friendly message (or fall back to `error.message`) and toast via `sonner`.
- Network failures → synthetic code (e.g., `"Network.Offline"`) with a generic toast.

## TanStack Query config

In `query-client.ts`:

```ts
new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error) => {
        if (error instanceof ApiError && error.status < 500) return false
        return failureCount < 2
      },
      staleTime: 60_000,
      refetchOnWindowFocus: false,
    },
  },
})
```

Don't retry on 4xx — they don't get better by retrying.

## SSE / realtime

For SignalR notifications, see `features/notifications/hooks/use-realtime-notifications.ts`. Don't add another transport. To push fresh data into the query cache, use `queryClient.setQueryData` or `invalidateQueries`.

## Forbidden

- `fetch(...)` calls outside `shared/api/`.
- Reading tokens from `localStorage` in components.
- Hard-coded URLs in components / hooks.
- Inline query keys at call sites (use `query-keys.ts`).
- Returning `any` from API functions.
- Separate fetch wrappers per feature.
- Retrying mutations automatically on 4xx.

## Done when

- New endpoint is one `httpClient.*` call from a typed wrapper.
- Query keys are in the feature's `query-keys.ts`.
- Hooks invalidate / update related caches on mutation success.
- Errors propagate as `ApiError` with `code` and `fieldErrors` populated.
- Auth refresh + retry behavior unchanged for happy paths.
- `pnpm typecheck` + `pnpm lint` pass.
