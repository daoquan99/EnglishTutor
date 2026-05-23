# Design System Architecture

## Core

Use:

```text
Tailwind CSS
shadcn/ui
lucide-react
```

## shadcn/ui Placement

Generated primitives should live in:

```text
src/shared/components/ui
```

Utility helper:

```text
src/shared/lib/utils.ts
```

## components.json

Recommended aliases:

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
