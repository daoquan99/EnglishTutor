# Skill: Create or Update UI Components

Use this skill when building shared or feature UI components.

## Component Placement

Shared primitive:

```text
src/shared/components/ui
```

Shared app component:

```text
src/shared/components
```

Feature component:

```text
src/features/{feature}/components
```

## Checklist

- Use TypeScript props.
- Avoid `any`.
- Support className when useful.
- Support loading/disabled states when relevant.
- Keep accessibility in mind.
- Keep component focused.
- Use shadcn/ui primitives when useful.
- Use Tailwind CSS for styling.
