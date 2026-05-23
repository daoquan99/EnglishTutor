---
name: ui-component
description: Adds or extends a UI component — picks the right placement (shadcn primitive vs feature composition), respects design tokens, and keeps accessibility correct.
---

# UI Component

## When to invoke

- New UI primitive needed across multiple features.
- New feature-specific component.
- Variant of an existing primitive.

## Decision tree

```
Is it a generic primitive used by 2+ features?
├── Yes → shadcn primitive in src/shared/components/ui/
│         If shadcn registry has it: `pnpm dlx shadcn@latest add <name>`
│         If not: write it in `shared/components/ui/` following shadcn patterns.
└── No  → feature component in src/features/{feature}/components/
          Compose shadcn primitives + Tailwind.
          Don't fork the primitive itself.
```

## Steps

### 1. Confirm placement

- Cross-feature → `src/shared/components/ui/`.
- App shells (layout / nav / sidebar) → `src/shared/components/`.
- Feature-specific → `src/features/{feature}/components/`.

### 2. Add via shadcn when possible

```bash
cd EnglishTutor.Web
pnpm dlx shadcn@latest add button   # example
```

Generated files land in `src/shared/components/ui/`. Don't heavily modify them — extend at the call site or with thin wrappers in the feature.

### 3. Use existing tokens

- Spacing: `p-1 .. p-12`, `gap-*`, `space-*` — pick from the existing scale.
- Radius: `rounded-*` tokens — don't introduce custom radii.
- Color: theme tokens via CSS variables. Use `text-foreground`, `bg-background`, `text-muted-foreground`, accent classes. No raw hex.
- Typography: `text-sm / text-base / text-lg` etc. Don't change `font-size` ad hoc.
- Merge classes with `cn(...)` from `@/shared/lib/utils`.

### 4. Variants

For multi-variant components use `class-variance-authority` (CVA):

```ts
const buttonVariants = cva('base classes', {
  variants: {
    variant: { default: '...', destructive: '...', ghost: '...' },
    size:    { sm: '...', default: '...', lg: '...' },
  },
  defaultVariants: { variant: 'default', size: 'default' },
})
```

### 5. Accessibility

- Use semantic HTML (`<button>` for actions, `<a>` for navigation).
- Label every interactive element (visible label or `aria-label`).
- Icons-only buttons need `aria-label` + `lucide-react` icon.
- Keyboard: every action reachable via Tab; Enter/Space activates buttons; Esc closes dialogs.
- Color contrast WCAG AA in both light/dark.
- Don't capture `Tab` for in-component navigation unless implementing a roving tabindex pattern.

### 6. Dark mode

- Use Tailwind dark variants (`dark:bg-*`, `dark:text-*`). Don't fork components.
- Test both themes in the dev server.

### 7. Composition over forking

If a feature needs a tinted/labeled card:

```tsx
// features/vocabulary/components/study-card.tsx
import { Card, CardHeader, CardContent } from '@/shared/components/ui/card'

export function StudyCard({ ... }) {
  return (
    <Card className="border-primary/30 bg-primary/5">
      <CardHeader>...</CardHeader>
      <CardContent>...</CardContent>
    </Card>
  )
}
```

Don't add a `tinted` prop to the shared `Card`.

### 8. Animations

- Use `tw-animate-css` utilities (already in deps).
- Animate `transform` / `opacity` only. Never animate `width`/`height`/`top`/`left` of layout-affecting elements.
- Keep durations short (150–300ms) and easing subtle.

### 9. Forms

- Inputs go through shadcn `Input` / `Textarea` / `Select` / `Checkbox` / `RadioGroup`.
- Wire to react-hook-form via shadcn `Form` components. Validate with Zod `zodResolver`.
- Disable submit while pending. Show field errors below inputs.

## Commands

```bash
cd EnglishTutor.Web
pnpm dev
pnpm typecheck
pnpm lint
```

## Done when

- Component placed correctly (shared/ui vs feature).
- Uses design tokens — no raw hex / custom px sizes.
- Accessible (keyboard + labels + contrast).
- Works in both light and dark mode.
- `pnpm typecheck` + `pnpm lint` pass.
- No other UI framework imported.
