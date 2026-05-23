# UI / design system rules

shadcn/ui + Tailwind v4. Clean, modern, calm, learning-focused. Suitable for adult learners.

## Component placement

| Location                              | Contents                                              |
| ------------------------------------- | ----------------------------------------------------- |
| `src/shared/components/ui/`           | shadcn primitives. Generic. Reusable.                 |
| `src/shared/components/`              | App-level shells (layout, header, nav, theme switch). |
| `src/features/{feature}/components/`  | Feature-specific composition.                         |

## shadcn aliases (`components.json`)

```json
{
  "aliases": {
    "components": "@/shared/components",
    "ui":         "@/shared/components/ui",
    "utils":      "@/shared/lib/utils",
    "lib":        "@/shared/lib",
    "hooks":      "@/shared/hooks"
  }
}
```

Use these aliases — don't import via deep relative paths.

## Adding a component

1. **Need a generic primitive?** → `pnpm dlx shadcn@latest add <component>` → lives in `src/shared/components/ui/`.
2. **Need a feature-specific component?** → put it in `src/features/{feature}/components/`. Compose shadcn primitives.
3. **Need a small variant of a primitive (e.g. tinted card)?** → use Tailwind classes at the call site, or a thin wrapper *inside the feature*, not by mutating the shared primitive.

## Tailwind

- Utility classes for everything. No feature-specific CSS files.
- Use the existing design tokens: spacing scale, radius scale, typography scale, color tokens. Don't introduce one-off hex values.
- Dark mode uses `next-themes`. Use Tailwind dark variants — don't fork components.
- Responsive: prefer Tailwind responsive utilities (`md:` / `lg:`). Avoid custom media queries.
- Use `tailwind-merge` (`cn(...)` helper in `shared/lib/utils.ts`) to merge classes safely.

## Design direction

Should feel:
- Clean, modern, calm.
- Friendly but professional.
- Learning-focused. Reduce visual noise.

Avoid:
- Overly playful "kids' app" UI.
- Heavy gradients or shadows.
- Many accent colors. Pick one primary + neutral grayscale.
- Inconsistent radii / shadows / spacings between screens.
- Generic admin-template look.
- Bouncy / spring-loaded animations everywhere. Animation should support the content, not distract.

## Patterns

- Loading: skeleton placeholders that match final layout (don't shift). Spinner only for inline buttons / small areas.
- Empty states: friendly one-liner + a primary CTA. No 404 art unless it's the actual 404 page.
- Errors: toast (`sonner`) for transient errors; inline form errors via react-hook-form's `formState.errors`; full-screen error boundary only for unrecoverable failures.
- Forms: react-hook-form + Zod (`zodResolver`). Validate on submit; show field errors below the input; disable submit while pending.
- Confirm dialogs: shadcn `AlertDialog`. Default to opting in for destructive actions.

## Accessibility

- All interactive elements reachable via keyboard.
- Forms have labels (`<label>` + `htmlFor`) — placeholder is not a label.
- Color contrast meets WCAG AA in both light and dark themes.
- Use semantic HTML (`<button>` for actions, `<a>` for navigation).

## Realtime (SignalR notifications)

- Connection managed in `features/notifications/hooks/use-realtime-notifications.ts`.
- Don't open multiple connections. One global hub connection per session.
- Reconnect logic uses the SignalR client's built-in policy.
- Updates flow into TanStack Query cache via `setQueryData` / `invalidateQueries`.

## Forbidden

- Adding heavy UI frameworks (MUI / AntD / Chakra / Mantine / daisyUI).
- Importing icons from elsewhere — use `lucide-react`.
- Toast libraries other than `sonner`.
- Chart libraries other than `recharts` for standard charts.
- Inline hex / rgb values in components (use Tailwind tokens or CSS variables).
- Mutating shared shadcn primitives for one-off feature needs (compose at the feature instead).
- Animations that affect layout (jank). Use transforms / opacity.
