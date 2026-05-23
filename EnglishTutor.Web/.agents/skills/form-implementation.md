---
name: form-implementation
description: Implements a validated form — react-hook-form + Zod schema + shadcn Form components + server error mapping. Covers both simple forms and multi-step/complex flows.
---

# Form Implementation

## When to invoke

- New form (create, edit, settings, multi-step wizard).
- Adding form validation (client + server).
- Wiring server-side field errors to the form UI.
- Converting uncontrolled inputs to a validated form.

## Stack

- **react-hook-form** — form state, field registration, submission.
- **Zod** — schema definition and validation.
- **@hookform/resolvers/zod** — bridge between RHF and Zod.
- **shadcn/ui Form components** — `Form`, `FormField`, `FormItem`, `FormLabel`, `FormControl`, `FormMessage`.

## Step-by-step

### 1. Define the Zod schema

`src/features/{feature}/schemas/{feature}-schemas.ts`:

```ts
import { z } from 'zod'

export const reviewSettingsSchema = z.object({
  newWordsPerDay: z.number().int().min(1).max(100),
  reviewWordsPerDay: z.number().int().min(1).max(200),
  includeMasteredInReview: z.boolean(),
})

export type ReviewSettingsFormValues = z.infer<typeof reviewSettingsSchema>
```

Rules:
- One schema per form (or per distinct form shape).
- Use `z.infer<typeof schema>` for the type — don't duplicate manually.
- Validation messages can be localized: `.min(1, { message: 'Tối thiểu 1' })`.
- Keep schemas in the feature's `schemas/` folder, not in components.

### 2. Create the form component

`src/features/{feature}/components/{feature}-form.tsx`:

```tsx
'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { reviewSettingsSchema, type ReviewSettingsFormValues } from '../schemas/{feature}-schemas'
import { Button } from '@/shared/components/ui/button'
import { Input } from '@/shared/components/ui/input'
import { Checkbox } from '@/shared/components/ui/checkbox'
import {
  Form,
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
} from '@/shared/components/ui/form'

type Props = {
  defaultValues?: Partial<ReviewSettingsFormValues>
  onSubmit: (values: ReviewSettingsFormValues) => void
  isPending?: boolean
}

export function ReviewSettingsForm({ defaultValues, onSubmit, isPending }: Props) {
  const form = useForm<ReviewSettingsFormValues>({
    resolver: zodResolver(reviewSettingsSchema),
    defaultValues: {
      newWordsPerDay: 10,
      reviewWordsPerDay: 30,
      includeMasteredInReview: false,
      ...defaultValues,
    },
  })

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="newWordsPerDay"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Từ mới mỗi ngày</FormLabel>
              <FormControl>
                <Input type="number" {...field} onChange={(e) => field.onChange(+e.target.value)} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        {/* ... more fields */}
        <Button type="submit" disabled={isPending}>
          {isPending ? 'Đang lưu...' : 'Lưu'}
        </Button>
      </form>
    </Form>
  )
}
```

### 3. Wire submission via mutation hook

```tsx
// In the parent component or page:
const mutation = useUpdateStudySettings()

function handleSubmit(values: ReviewSettingsFormValues) {
  mutation.mutate(values, {
    onSuccess: () => toast.success('Đã lưu cài đặt'),
    onError: (error) => {
      if (error instanceof ApiError && error.fieldErrors) {
        // Map server field errors to form
        Object.entries(error.fieldErrors).forEach(([field, messages]) => {
          form.setError(field as keyof ReviewSettingsFormValues, {
            message: messages[0],
          })
        })
      } else {
        toast.error(error.message)
      }
    },
  })
}
```

### 4. Map server validation errors

Backend returns ProblemDetails with `errors` field as `Record<string, string[]>`. The `ApiError` class exposes this as `fieldErrors`.

Pattern for mapping:

```ts
import { ApiError } from '@/shared/api/api-error'
import type { UseFormReturn } from 'react-hook-form'

export function mapServerErrors<T extends Record<string, unknown>>(
  error: unknown,
  form: UseFormReturn<T>,
): boolean {
  if (error instanceof ApiError && error.fieldErrors) {
    Object.entries(error.fieldErrors).forEach(([field, messages]) => {
      const key = camelCase(field) as keyof T  // backend uses PascalCase
      form.setError(key as any, { message: messages[0] })
    })
    return true
  }
  return false
}
```

### 5. Loading / disabled states

- Disable the submit button while `isPending`.
- Show a spinner icon inside the button (use `Loader2` from lucide-react).
- Don't disable the entire form — users should be able to read/copy field values while submitting.

### 6. Multi-step forms (if applicable)

- Use a state machine (`useState` or `useReducer`) for step tracking.
- Validate per-step with partial schemas: `schema.pick({ field1: true, field2: true })`.
- Only submit the full payload on the final step.

## Field patterns

| Input type      | Component                           | Notes                                    |
| --------------- | ----------------------------------- | ---------------------------------------- |
| Text            | `Input`                             |                                          |
| Number          | `Input type="number"` + `onChange`  | Parse to number in onChange              |
| Textarea        | `Textarea`                          |                                          |
| Select          | shadcn `Select`                     | Use `FormField` + `SelectTrigger`        |
| Checkbox        | shadcn `Checkbox`                   | `onCheckedChange` maps to `field.onChange`|
| Date            | shadcn `Calendar` + `Popover`       | Store as ISO string with Utc suffix      |
| File/Audio      | Custom `<input type="file">`        | Handle via `FormData`, not form state    |
| Password        | `PasswordInput` from `shared/`      | Toggle visibility                        |

## Forbidden

- Defining schemas inline in components — put them in `schemas/`.
- Using `any` in form types.
- Swallowing server errors without displaying them.
- Submitting without `form.handleSubmit` (bypasses validation).
- Calling the API directly from the form component — use a mutation hook.
- Duplicating the Zod type manually instead of using `z.infer`.

## Commands

```bash
cd EnglishTutor.Web
pnpm dev
pnpm typecheck
pnpm lint
```

## Done when

- Form validates on submit (client-side via Zod).
- Field errors display below the relevant input.
- Server validation errors map to the correct fields.
- General errors show via toast (`sonner`).
- Submit button shows pending state.
- `pnpm typecheck` + `pnpm lint` pass.
- No `any` in form/schema types.
