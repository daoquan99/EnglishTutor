# Skill: Build Audio and Speaking UI

Use this skill for speaking practice, pronunciation attempts, and recording UI.

## Browser APIs

May use:

```text
MediaRecorder API
Web Audio API
navigator.mediaDevices.getUserMedia
```

## UX Requirements

Always handle:

```text
microphone permission denied
microphone unavailable
recording
paused/stopped
uploading
analyzing
success
error
retry
```

## Architecture

- Keep recording logic inside feature hooks.
- Keep UI components focused on display and interaction.
- Store audio upload API calls inside feature API clients.
- Do not put browser API code inside page files.
