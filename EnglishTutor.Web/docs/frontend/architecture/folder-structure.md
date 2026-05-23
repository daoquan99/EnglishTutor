# Frontend Folder Structure

Recommended structure:

```text
src
├── app
│   ├── (public)
│   ├── (auth)
│   ├── (app)
│   ├── (admin)
│   ├── layout.tsx
│   └── globals.css
│
├── features
└── shared
```

## src/app

Use for:

```text
routes
layouts
loading.tsx
error.tsx
not-found.tsx
page composition
metadata
```

Do not put large business logic here.

## src/features

Use for business features.

## src/shared

Use only for generic reusable code.
