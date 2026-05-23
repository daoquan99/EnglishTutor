# API Client Rules

## Shared HTTP Client

All backend calls must go through:

```text
src/shared/api/http-client.ts
```

Feature API files should import and use the shared client.

Do not call `fetch` directly from random page or component files.

## Suggested Shared API Structure

```text
src/shared/api
├── http-client.ts
├── api-error.ts
├── auth-token-store.ts
├── query-client.ts
└── index.ts
```

## Error Handling

Normalize errors into a shared shape:

```ts
type ApiError = {
  status: number
  code?: string
  message: string
  details?: unknown
}
```

## Query Keys

Keep query keys colocated with the feature.
