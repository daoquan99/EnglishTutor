---
name: realtime-notifications
description: Integrates realtime push from backend via SignalR — connection lifecycle, event handling, query cache invalidation, and toast display. Uses the centralized notification hub connection.
---

# Realtime Notifications

## When to invoke

- New notification type pushed from backend needs frontend handling.
- Wiring a feature to react to realtime events (e.g., exercise scored, speaking session analyzed).
- Changing SignalR connection lifecycle or reconnect behavior.
- Adding toast/badge updates from pushed events.

## Architecture

```
src/features/notifications/
├── api/
│   ├── notifications-api.ts         # REST endpoints for notification list, mark-read
│   └── query-keys.ts
├── components/
│   ├── notification-bell.tsx         # Badge icon in app header
│   ├── notification-list.tsx         # Full notification list
│   ├── notification-popover.tsx      # Dropdown from bell
│   ├── notification-provider.tsx     # Mounts useRealtimeNotifications
│   └── notification-settings-form.tsx
├── hooks/
│   ├── use-realtime-notifications.ts # Core hook — connects, listens, invalidates
│   ├── use-notifications.ts          # TanStack Query for notification list
│   ├── use-mark-notification-read.ts
│   ├── use-mark-all-read.ts
│   └── use-notification-settings.ts
├── lib/
│   ├── signalr-connection.ts         # Singleton HubConnection factory
│   ├── toast-helpers.ts              # Maps notification payload to sonner toasts
│   └── notification-icons.tsx        # Icon mapping per notification type
└── types/
    └── notifications.ts
```

## SignalR connection

**Singleton pattern** in `lib/signalr-connection.ts`:

```ts
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { getAccessToken } from '@/shared/api/auth-token-store'

let connection: HubConnection | null = null

export function getNotificationConnection(): HubConnection {
  if (!connection) {
    connection = new HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/notifications`, {
        accessTokenFactory: () => getAccessToken() ?? '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build()
  }
  return connection
}

export function disposeNotificationConnection(): void {
  if (connection) {
    connection.stop()
    connection = null
  }
}
```

Rules:
- **One connection** for the entire app. Don't create additional hub connections from feature modules.
- Token is fetched from `auth-token-store` via `accessTokenFactory` — no manual header setting.
- Automatic reconnect with exponential backoff array.
- Dispose on user logout or component unmount.

## Core hook pattern

`hooks/use-realtime-notifications.ts` does three things:

1. **Connect** — start the hub if disconnected.
2. **Listen** — `connection.on('ReceiveNotification', handler)`.
3. **Invalidate** — on each push, invalidate notification query keys + any feature-specific keys.

```ts
export function useRealtimeNotifications() {
  const queryClient = useQueryClient()
  const user = useAuthStore((s) => s.user)

  useEffect(() => {
    if (!user) return
    const connection = getNotificationConnection()

    const handleReceive = (raw: string) => {
      const payload = JSON.parse(raw) as NotificationPushPayload
      queryClient.invalidateQueries({ queryKey: notificationKeys.all })
      showNotificationToast(payload)
    }

    connection.on('ReceiveNotification', handleReceive)
    connection.onreconnected(() => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all })
    })

    if (connection.state === HubConnectionState.Disconnected) {
      connection.start().catch(() => { /* retry handled by withAutomaticReconnect */ })
    }

    return () => {
      connection.off('ReceiveNotification', handleReceive)
      disposeNotificationConnection()
    }
  }, [queryClient, user])
}
```

## Adding a new notification type

### 1. Add the type to `types/notifications.ts`

```ts
export type NotificationType =
  | 'study_reminder'
  | 'achievement_unlocked'
  | 'exercise_scored'     // ← new
  // ...
```

### 2. Add icon mapping in `lib/notification-icons.tsx`

### 3. Add toast mapping in `lib/toast-helpers.ts`

```ts
export function showNotificationToast(payload: NotificationPushPayload) {
  switch (payload.type) {
    case 'exercise_scored':
      toast.success(`Bài tập đã được chấm: ${payload.title}`)
      break
    // ...
  }
}
```

### 4. Invalidate feature-specific queries (optional)

If the notification should trigger a data refresh in a specific feature, extend the `handleReceive` callback:

```ts
if (payload.type === 'exercise_scored') {
  queryClient.invalidateQueries({ queryKey: exerciseKeys.all })
}
```

## Mounting the provider

`notification-provider.tsx` is mounted in the `(app)` layout so all authenticated routes receive pushes:

```tsx
// src/app/(app)/layout.tsx
import { NotificationProvider } from '@/features/notifications/components/notification-provider'

export default function AppLayout({ children }) {
  return (
    <>
      <NotificationProvider />
      {children}
    </>
  )
}
```

## Forbidden

- Opening a second SignalR connection from another feature.
- Polling the server for notifications — use the push.
- Storing pushed payloads in localStorage — use the query cache.
- Starting the connection before the user is authenticated.
- Blocking the UI on connection failure — degrade gracefully.

## Commands

```bash
cd EnglishTutor.Web
pnpm dev
pnpm typecheck
pnpm lint
```

## Done when

- New notification type appears as a toast on push.
- Notification list/bell updates without page refresh.
- Feature-specific queries invalidated if applicable.
- No new SignalR connection created.
- `pnpm typecheck` + `pnpm lint` pass.
- Connection cleanly disposes on logout.
