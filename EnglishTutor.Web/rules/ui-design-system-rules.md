# UI Design System Rules

## Direction

EnglishTutor UI should feel:

```text
clean
modern
calm
focused
friendly
professional
SaaS-ready
```

The product is for serious adult learners and developers, not children.

## Core Tools

Use:

```text
Tailwind CSS
shadcn/ui
lucide-react
```

Do not add another UI system unless explicitly requested.

## Component Levels

UI primitives:

```text
src/shared/components/ui
```

Shared app components:

```text
src/shared/components
```

Feature components:

```text
src/features/{feature}/components
```

## Visual Consistency

- Use consistent card radius.
- Use consistent shadows.
- Use consistent page max width.
- Use consistent section spacing.
- Use consistent typography scale.
- Prefer neutral base with one primary accent.
- Avoid rainbow dashboards.
- Avoid excessive gradients.
