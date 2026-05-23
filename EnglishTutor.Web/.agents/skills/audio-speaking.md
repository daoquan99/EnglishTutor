---
name: audio-speaking
description: Implements audio-based UX for the Speaking feature — recording, uploading, transcript display, per-turn correction UI, playback. Coordinates with backend Speaking endpoints.
---

# Audio / Speaking

## When to invoke

- New speaking session UX (start / turn submit / complete).
- Recording / playback controls.
- Transcript + correction display.
- Pronunciation practice for vocabulary example sentences.

## Backend touch points

Read the canonical contract: `docs/api/speaking.md` (BE) and the workflow `EnglishTutor.API/.agents/workflows/speaking-session.md`.

Endpoints (representative — confirm in docs):

- `POST /speaking/sessions` — create session, returns `sessionId` + first prompt.
- `POST /speaking/sessions/{id}/turns` — upload audio, get transcript + correction.
- `POST /speaking/sessions/{id}/complete` — finalize, get session summary.

## Browser audio capture

Use `MediaRecorder` over `getUserMedia`:

```ts
const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
const recorder = new MediaRecorder(stream, { mimeType: 'audio/webm;codecs=opus' })
const chunks: Blob[] = []
recorder.ondataavailable = (e) => chunks.push(e.data)
recorder.onstop = () => {
  const blob = new Blob(chunks, { type: recorder.mimeType })
  // upload blob
  stream.getTracks().forEach(t => t.stop())
}
recorder.start()
```

- Always release the stream tracks when the recorder stops — otherwise the mic indicator stays on.
- Provide a visible recording indicator. Provide a stop button.
- Cap recording length (e.g., 60s per turn) with an auto-stop.

## Permissions

- Before recording, check permission via the Permissions API (`navigator.permissions.query({ name: 'microphone' })`).
- If denied, show a clear instructional state with a "How to enable" hint. Don't loop the permission prompt.

## Upload

Audio is `multipart/form-data` to the turn endpoint, with the audio Blob and metadata fields:

```ts
const form = new FormData()
form.append('audio', blob, 'turn.webm')
form.append('expectedText', expectedText ?? '')
await httpClient.postForm(`/speaking/sessions/${sessionId}/turns`, form)
```

Add a `httpClient.postForm` helper if not present — it should skip JSON serialization, keep auth header.

## Playback

- Use a small custom player around `<audio>` for play/pause/scrubbing. Don't use a heavy media library.
- Backend returns a short-lived signed URL for stored audio. Don't cache the URL in long-lived state; refetch via the parent query when expired.
- Surface a "play original / play my recording" toggle when comparing pronunciation.

## Transcript + correction UI

- Render the user's transcript and the corrected version side-by-side or stacked.
- Highlight inline diffs (insert / delete / replace). Use neutral colors; this is feedback, not a quiz.
- Per-turn scores (pronunciation, fluency, grammar) render as compact badges/bars.

## TanStack Query patterns

- Session query: `['speaking', 'session', sessionId]`. Mutation invalidates this key after each turn.
- Mutation for `submitTurn`: pessimistic UI (show "scoring..." state) — the response shapes the next turn.
- Mutation for `completeSession`: invalidates session query and `['progress', 'dashboard']` so the dashboard reflects new EXP.

## State machine

```
idle → recording → recorded → uploading → scored → idle (next turn)
                                                       │
                                                       └─→ completing → completed
```

Use a small reducer (`useReducer`) or Zustand store for this — not scattered `useState` flags.

## Accessibility

- Keyboard: Space toggles record start/stop (with clear visible state).
- Live region (`aria-live="polite"`) announces "Recording started" / "Recording stopped" / "Scoring..." for screen readers.
- Provide a text-input fallback path (for users who can't use mic) — typed answer goes through a different endpoint or a `submitType` field in the turn payload.

## Performance

- Don't re-encode audio in JS. Send the recorded blob directly.
- Don't hold raw PCM in memory. Use `MediaRecorder` and stream the blob chunks.
- Lazy-load the speaking feature route (it's heavier than other features).

## Forbidden

- Capturing audio without an explicit user action (mic permission must be triggered by a button click).
- Storing audio blobs in localStorage/sessionStorage.
- Leaving the mic stream open after recording stops.
- Polling the server for transcript — the turn endpoint returns synchronously (or via a streamed response if backend introduces SSE; coordinate first).
- Forking the SignalR connection from `features/notifications/`.

## Done when

- Recording → upload → score works in `pnpm dev`.
- Mic indicator releases after stop.
- Permission denial shows a helpful empty state.
- `pnpm typecheck` + `pnpm lint` pass.
- Session UI degrades gracefully on AI failure (typed error, can retry the turn).
